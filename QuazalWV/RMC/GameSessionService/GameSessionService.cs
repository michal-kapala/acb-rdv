using System;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public class GameSessionService
    {
        public static void ProcessRequest(Stream s, RMCP rmc, ClientInfo client)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestGameSessionService_CreateSession(s);
                    Log.WriteLine(1, "[RMC GameSession] CreateSession props:\n" + rmc.request.PayloadToString(), Color.Blue, client);
                    break;
                case 2:
                    rmc.request = new RMCPacketRequestGameSessionService_UpdateSession(s);
                    Log.WriteLine(1, "[RMC GameSession] UpdateSession props:\n" + rmc.request.PayloadToString(), Color.Purple, client);
                    break;
                case 4:
                    rmc.request = new RMCPacketRequestGameSessionService_MigrateSession(s);
                    break;
                case 5:
                    rmc.request = new RMCPacketRequestGameSessionService_LeaveSession(s);
                    Log.WriteLine(1, "[RMC GameSession] LeaveSession key:\n" + rmc.request.PayloadToString(), Color.Purple, client);
                    break;
                case 7:
                    rmc.request = new RMCPacketRequestGameSessionService_SearchSessions(s);
                    Log.WriteLine(1, "[RMC GameSession] SearchSessions query props:\n" + rmc.request.PayloadToString(), Color.Orange, client);
                    break;
                case 8:
                    rmc.request = new RMCPacketRequestGameSessionService_AddParticipants(s);
                    break;
                case 9:
                    rmc.request = new RMCPacketRequestGameSessionService_RemoveParticipants(s);
                    break;
                case 12:
                    rmc.request = new RMCPacketRequestGameSessionService_SendInvitation(s);
                    break;
                case 14:
                    rmc.request = new RMCPacketRequestGameSessionService_GetInvitationsReceived(s);
                    break;
                case 19:
                    rmc.request = new RMCPacketRequestGameSessionService_CancelInvitation(s);
                    break;
                case 21:
                    rmc.request = new RMCPacketRequestGameSessionService_RegisterURLs(s);
                    break;
                case 23:
                    rmc.request = new RMCPacketRequestGameSessionService_AbandonSession(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC GameSession] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    var reqCreateSes = (RMCPacketRequestGameSessionService_CreateSession)rmc.request;
                    uint sesId = Global.NextGameSessionId++;
                    client.GameSessionID = sesId;
                    client.InGameSession = true;
                    var ses = new Session(sesId, reqCreateSes.Session, client);
                    Log.WriteLine(1, $"[RMC GameSession] New session (id={ses.Key.SessionId})", Color.Blue, client);
                    // initialize params
                    var gameType = ses.GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameType);
                    if (gameType == null)
                        Log.WriteLine(1, $"[Session] Inconsistent session state (id={ses.Key.SessionId}), missing game type", Color.Red, client);
                    var currPublicSlots = new Property() { Id = (uint)SessionParam.CurrentPublicSlots, Value = 0 };
                    var currPrivateSlots = new Property() { Id = (uint)SessionParam.CurrentPrivateSlots, Value = 0 };
                    var accessibility = new Property() { Id = (uint)SessionParam.Accessibility, Value = 0 };
                    ses.GameSession.Attributes.Add(currPublicSlots);
                    ses.GameSession.Attributes.Add(currPrivateSlots);
                    ses.GameSession.Attributes.Add(accessibility);
                    // blind NAT type update
                    var natType = ses.GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.NatType);
                    if (natType == null)
                        Log.WriteLine(1, $"[Session] Inconsistent session state (id={ses.Key.SessionId}), missing NAT type", Color.Red, client);
                    natType.Value = (uint)NatType.OPEN;
                    Global.Sessions.Add(ses);
                    reply = new RMCPacketResponseGameSessionService_CreateSession(reqCreateSes.Session.TypeId, sesId);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 2:
                    var reqUpdateSes = (RMCPacketRequestGameSessionService_UpdateSession)rmc.request;
                    Global.Sessions.Find(session => session.Key.SessionId == reqUpdateSes.SessionUpdate.Key.SessionId)
                        .GameSession.Attributes = reqUpdateSes.SessionUpdate.Attributes;
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 4:
                    Log.WriteLine(1, $"[RMC GameSession] Migrate Session started",Color.Blue);
                    var reqMigrate = (RMCPacketRequestGameSessionService_MigrateSession)rmc.request;
                    // TODO: select new session, for now return the old key
                    reply = new RMCPacketResponseGameSessionService_MigrateSession(reqMigrate.Key);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                    //todo 
                    Session migratefrom = Global.Sessions.Find(session => session.Key.SessionId == client.GameSessionID);
                    
                    Log.WriteLine(1, $"[RMC GameSession] Migrate Session migratedkey rcvd to {((RMCPacketResponseGameSessionService_MigrateSession)reply).MigratedKey.SessionId}",Color.Red);
                    Log.WriteLine(1, $"[RMC GameSession] Migrate Session from (id={client.GameSessionID}", Color.Blue, client);
                    Session migrateto = Global.Sessions.Find(session => session.Key.SessionId == ((RMCPacketResponseGameSessionService_MigrateSession)reply).MigratedKey.SessionId);
                    if (client.InGameSession==false)
                    {
                        Log.WriteLine(1, $"[RMC GameSession] Client not in session");
                        client.GameSessionID = migrateto.Key.SessionId;
                        client.InGameSession = true;
                    }
                    else if (migratefrom==null )
                    {
                        Log.WriteLine(1, $"[RMC GameSession] Migrate Session cannot find session to migrate from");
                    }
                    if (migrateto.GameSession == null)
                    {
                        Log.WriteLine(1, $"[RMC GameSession] Session not existant");
                        Log.WriteLine(1, $"[RMC GameSession] Migrate Session (id={migrateto})");
                    }
                    if (migrateto!=null && migratefrom!=null)
                    {
                        Log.WriteLine(1, $"[RMC GameSession] Migrate Session (id={((RMCPacketResponseGameSessionService_MigrateSession)reply).MigratedKey.SessionId}) from {migratefrom.Key.SessionId}", Color.Blue, client);
                        client.GameSessionID = migrateto.Key.SessionId ;
                    }

                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 5:
                    Log.WriteLine(1, $"[RMC GameSession] Leave Session");
                    var reqLeaveSes = (RMCPacketRequestGameSessionService_LeaveSession)rmc.request;
                    var leftSes = Global.Sessions.Find(session => session.Key.SessionId == reqLeaveSes.Key.SessionId);
                    // delete the session if empty
                    if (leftSes.NbParticipants() == 1)
                    {
                        Global.Sessions.Remove(leftSes);
                        Log.WriteLine(1, $"[RMC GameSession] Session {reqLeaveSes.Key.SessionId} deleted on leave from player {client.PID}", Color.Gray, client);
                    }
                    else
                        leftSes.Leave(client);
                    reply = new RMCPResponseEmpty();
                    client.GameSessionID = 0xffffffff;
                    client.InGameSession = false;
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 7:
                    var reqSearchSes = (RMCPacketRequestGameSessionService_SearchSessions)rmc.request;
                    Log.WriteLine(2, $"[RMC] SearchSessions query: {reqSearchSes.Query}", Color.Green, client);
                    reply = new RMCPacketResponseGameSessionService_SearchSessions(reqSearchSes.Query, client);
                    Log.WriteLine(2, $"[RMC] SearchSessions results: {((RMCPacketResponseGameSessionService_SearchSessions)reply).Results.Count}", Color.Green, client);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 8:
                    var reqAddParticip = (RMCPacketRequestGameSessionService_AddParticipants)rmc.request;
                    Log.WriteLine(1, $"[RMC] Addparticipants pids results: {string.Join(", ", reqAddParticip.PublicPids)}", Color.Green, client);
                    Global.Sessions.Find(session => session.Key.SessionId == reqAddParticip.Key.SessionId)
                        .AddParticipants(reqAddParticip.PublicPids, reqAddParticip.PrivatePids);
                    foreach (uint i in reqAddParticip.PublicPids)
                    {
                        ClientInfo result = Global.Clients.Find(cli => cli.PID == i);
                        if (result!=null)
                        {
                            result.GameSessionID = reqAddParticip.Key.SessionId;
                            result.InGameSession = true;
                        }
                        else
                        {
                            Log.WriteLine(1, $"[RMC] Addparticipants pids results:could not find client verify1", Color.Green, client);
                        }
                    }
                    foreach (uint i in reqAddParticip.PrivatePids)
                    {
                        ClientInfo result = Global.Clients.Find(cli => cli.PID == i);
                        if (result != null)
                        {
                            result.GameSessionID = reqAddParticip.Key.SessionId;
                            result.InGameSession = true;
                        }
                        else
                        {
                            Log.WriteLine(1, $"[RMC] Addparticipants pids results:could not find client verify2", Color.Green, client);
                        }
                    }


                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 9:
                    var reqRemoveParticip = (RMCPacketRequestGameSessionService_RemoveParticipants)rmc.request;
                    Log.WriteLine(1, $"[RMC] Removeparticipants pids results: {reqRemoveParticip}", Color.Green, client);
                    Global.Sessions.Find(session => session.Key.SessionId == reqRemoveParticip.Key.SessionId)
                        .RemoveParticipants(reqRemoveParticip.PublicPids);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 12:
                    Log.WriteLine(1, $"[RMC] RemoveGameSessionService SendInvitation ", Color.Blue);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 14:
                    Log.WriteLine(1, $"[RMC] RemoveGameSessionService GetInvitationsReceived ",Color.Blue);
                    var reqGetInvRecv = (RMCPacketRequestGameSessionService_GetInvitationsReceived)rmc.request;
                    reply = new RMCPacketResponseGameSessionService_GetInvitationsReceived();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 19:
                    Log.WriteLine(1, $"[RMC] RemoveGameSessionService Cancel Invitation ", Color.Blue);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 21://
                    Log.WriteLine(1, $"[RMC] RemoveGameSessionService RegisterURLs", Color.Blue);
                    var reqRegUrls = (RMCPacketRequestGameSessionService_RegisterURLs)rmc.request;
                    reqRegUrls.RegisterUrls(client);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 23:
                    Log.WriteLine(1, $"[RMC] RemoveGameSessionService Abandon Session client pid {client.PID}", Color.Blue);
                    var reqAbandon = (RMCPacketRequestGameSessionService_AbandonSession)rmc.request;
                    var abandonedSes = Global.Sessions.Find(session => session.Key.SessionId == reqAbandon.Key.SessionId);
                    if (abandonedSes != null)
                    {
                        if (abandonedSes.NbParticipants() == 1)
                        {
                            Global.Sessions.Remove(abandonedSes);
                            Log.WriteLine(1, $"[RMC GameSession] Session {abandonedSes.Key.SessionId} deleted on abandon from player {client.PID}", Color.Gray, client);
                        }
                        else
                        {
                            bool notExist = false;
                            if (abandonedSes.PublicPids.Contains(client.PID))
                            {
                                abandonedSes.PublicPids.Remove(client.PID);
                            }
                            else
                            {
                                notExist = true;
                            }
                            if (abandonedSes.PrivatePids.Contains(client.PID))
                            {
                                abandonedSes.PrivatePids.Remove(client.PID);
                                notExist = false;
                            }
                            else if (notExist==true)
                            {
                                notExist = true;
                            }
                            if (notExist==true)
                            {
                                Log.WriteLine(1, $"[RMC GameSession] Pid not found in session", Color.Gray, client);
                            }
                        }
                        client.GameSessionID = 0xffffffff;
                        client.InGameSession = false;
                        //abandonedSes.PrivatePids.Remove(client.PID);
                        //if (abandonedSes.PrivatePids.Count == 0)
                        //    Global.Sessions.Remove(abandonedSes);
                    }
                    else
                    {
                        Log.WriteLine(1, $"[RMC GameSession] Session abandoned but it's null so should not be happening ");
                    }
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC GameSession] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
