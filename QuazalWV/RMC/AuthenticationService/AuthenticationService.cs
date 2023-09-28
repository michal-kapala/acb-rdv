using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

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
                    rmc.request = new RMCPacketRequestLoginCustomData(s);
                    break;
                case 3:
                    rmc.request = new RMCPacketRequestRequestTicket(s);
                    break;
                default:
                    Log.WriteLine(1, "[RMC Authentication] Error: Unknown Method 0x" + rmc.methodID.ToString("X"));
                    break;
            }
        }


        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    var loginReq = (RMCPacketRequestAuthenticationService_Login)rmc.request;
                    User u = DBHelper.GetUserByName(loginReq.username);
                    // 'Tracking' account (telemetry) needs to exist, users call LoginCustomData
                    if (u != null && loginReq.username == "Tracking")
                        client.TrackingUser = u;
                    else 
                        Log.WriteLine(1, $"[RMC Authentication] Login called for a non-existent user {loginReq.username}", Color.Red);

                    reply = new RMCPacketResponseAuthenticationService_Login(client);
                    //client.sessionKey = ((RMCPacketResponseAuthenticationService_Login)reply).ticket.sessionKey;
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, useCompression: false);
                    break;
                case 2:
                    RMCPacketRequestLoginCustomData h = (RMCPacketRequestLoginCustomData)rmc.request;
                    switch (h.className)
                    {
                        case "UbiAuthenticationLoginCustomData":
                            reply = new RMCPResponseEmpty();
                            User user = DBHelper.GetUserByName(h.username);
                            if (user != null)
                            {
                                if (user.Password == h.password)
                                {
                                    reply = new RMCPacketResponseLoginCustomData(client.PID, client.sPID, client.sPort);
                                    client.PID = user.Pid;
                                    client.User = user;
                                    client.sessionKey = ((RMCPacketResponseLoginCustomData)reply).ticket.sessionKey;
                                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                                }
                                else
                                {
                                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, 0x80030065);
                                }
                            }
                            else
                            {
                                Log.WriteLine(1, $"[RMC Authentication] LoginCustomData called for a non-existent user {h.username}", Color.Red);
                                RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, 0x80030064);
                            }
                            break;
                        default:
                            Log.WriteLine(1, "[RMC Authentication] Error: Unknown Custom Data class " + h.className);
                            break;
                    }
                    break;
                case 3:
                    var reqTicket = (RMCPacketRequestRequestTicket)rmc.request;
                    reply = new RMCPacketResponseRequestTicket(reqTicket.sourcePID, client.sPID);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, "[RMC Authentication] Error: Unknown Method 0x" + rmc.methodID.ToString("X"));
                    break;
            }
        }
    }
}
