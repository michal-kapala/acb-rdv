using System.IO;
using System.Drawing;
using System;

namespace QuazalWV
{
    public static class AuthenticationService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestAuthenticationService_Login(s);
                    break;
                case 2:
                    rmc.request = new RMCPacketRequestAuthenticationService_LoginEx(s);
                    break;
                case 3:
                    rmc.request = new RMCPacketRequestAuthenticationService_RequestTicket(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Authentication] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }


        public static void HandleRequest(PrudpPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    var reqLogin = (RMCPacketRequestAuthenticationService_Login)rmc.request;
                    try
                    {
                        User u = DbHelper.GetUserByName(reqLogin.username);
                        // 'Tracking' account (telemetry) needs to exist, users call LoginCustomData
                        if (u != null && reqLogin.username == "Tracking")
                            client.TrackingUser = u;
                        else
                            Log.WriteLine(1, $"[RMC Authentication] Login called for a non-existent user {reqLogin.username}", Color.Red);

                        reply = new RMCPacketResponseAuthenticationService_Login(client);
                        //client.sessionKey = ((RMCPacketResponseAuthenticationService_Login)reply).ticket.sessionKey;
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, $"[RMC Authentication] Login for Tracking: {ex}", Color.Red);
                    }
                    break;
                case 2:
                    RMCPacketRequestAuthenticationService_LoginEx reqLoginEx = (RMCPacketRequestAuthenticationService_LoginEx)rmc.request;
                    switch (reqLoginEx.className)
                    {
                        case "UbiAuthenticationLoginCustomData":
                            reply = new RMCPResponseEmpty();
                            User user = DbHelper.GetUserByName(reqLoginEx.username);
                            if (user != null)
                            {
                                if (user.Password == reqLoginEx.password)
                                {
                                    client.User = user;
                                    reply = new RMCPacketResponseAuthenticationService_LoginEx(client.User.Pid, client.sPID, client.sPort);
                                    client.sessionKey = ((RMCPacketResponseAuthenticationService_LoginEx)reply).ticket.sessionKey;
                                    Global.RemoveSessionsOnLogin(client);
                                    // TODO: kick everyone that has joined the sessions hosted by the guy who logged in again
                                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                                }
                                else
                                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.RendezVous_InvalidPassword);
                            }
                            else
                            {
                                Log.WriteLine(1, $"[RMC Authentication] LoginEx called for a non-existent user {reqLoginEx.username}", Color.Red, client);
                                RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.RendezVous_InvalidUsername);
                            }
                            break;
                        default:
                            Log.WriteLine(1, $"[RMC Authentication] Error: Unknown Custom Data class {reqLoginEx.className}", Color.Red, client);
                            break;
                    }
                    break;
                case 3:
                    var reqTicket = (RMCPacketRequestAuthenticationService_RequestTicket)rmc.request;
                    reply = new RMCPacketResponseAuthenticationService_RequestTicket(reqTicket.sourcePID, client);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    // reset sequence IDs for secure service connection
                    if (p.m_oSourceVPort.port == 15)
                    {
                        // player
                        client.seqIdReliable = 1;
                        client.seqIdUnreliable = 1;
                    }
                    else
                    {
                        // Tracking user
                        client.seqIdReliableTracking = 1;
                        client.seqIdUnreliableTracking = 1;
                    }
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Authentication] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
