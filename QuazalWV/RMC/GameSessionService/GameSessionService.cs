using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;


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
                case 6:
                    rmc.request = new RMCPacketRequestGameSessionService_GetSession(s);
                    Log.WriteLine(1, "[RMC GameSession] GetSession :\n" + rmc.request.PayloadToString(), Color.Purple, client);
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
                case 17:
                    rmc.request = new RMCPacketRequestGameSessionService_AcceptInvitation(s);
                    break;
                case 18:
                    rmc.request = new RMCPacketRequestGameSessionService_DenyInvitation(s);
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
        public static ClientInfo IsUserOnlinebyID(uint userID)
        {
            foreach (ClientInfo a in Global.Clients)
            {
                if (a.User.UserDBPid==userID)
                {
                    return a;
                }
            }
            return null;
        }
        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            uint sesId;
            Property gameType, currPublicSlots, currPrivateSlots, accessibility;
            Session newsession;
            Session migratefrom;
            switch (rmc.methodID)
            {
                case 1:
                    var reqCreateSes = (RMCPacketRequestGameSessionService_CreateSession)rmc.request;
                    sesId = Global.NextGameSessionId++;
                    client.GameSessionID = sesId;
                    client.InGameSession = true;
                    newsession = new Session(sesId, reqCreateSes.Session, client);
                    Log.WriteLine(1, $"[RMC GameSession] New session (id={newsession.Key.SessionId}) for client {client.User.Name}", Color.Blue, client);
                    // initialize params
                    gameType = newsession.GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameType);
                    if (gameType == null)
                        Log.WriteLine(1, $"[Session] Inconsistent session state (id={newsession.Key.SessionId}), missing game type", Color.Red, client);
                    currPublicSlots = new Property() { Id = (uint)SessionParam.CurrentPublicSlots, Value = 0 };
                    currPrivateSlots = new Property() { Id = (uint)SessionParam.CurrentPrivateSlots, Value = 0 };
                    accessibility = new Property() { Id = (uint)SessionParam.Accessibility, Value = 0 };
                    newsession.GameSession.Attributes.Add(currPublicSlots);
                    newsession.GameSession.Attributes.Add(currPrivateSlots);
                    newsession.GameSession.Attributes.Add(accessibility);
                    // blind NAT type update
                    var natType = newsession.GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.NatType);
                    if (natType == null)
                        Log.WriteLine(1, $"[Session] Inconsistent session state (id={newsession.Key.SessionId}), missing NAT type", Color.Red, client);
                    natType.Value = (uint)NatType.OPEN;
                    Global.Sessions.Add(newsession);
                    reply = new RMCPacketResponseGameSessionService_CreateSession(reqCreateSes.Session.TypeId, sesId);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 2:
                    var reqUpdateSes = (RMCPacketRequestGameSessionService_UpdateSession)rmc.request;
                    Global.Sessions.Find(session => session.Key.SessionId == reqUpdateSes.SessionUpdate.Key.SessionId)
                        .GameSession.Attributes = reqUpdateSes.SessionUpdate.Attributes;
                    Log.WriteLine(1, $"[RMC GameSession] Update session Attributes {string.Join(" | ", reqUpdateSes.SessionUpdate.Attributes.Select(par => string.Join(" ", par.GetType().GetProperties().Select(prop => prop.GetValue(par)))))} (id={reqUpdateSes.SessionUpdate.Key.SessionId}) for client {client.User.Name}", Color.Blue, client);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 4:
                    try
                    {
                        Log.WriteLine(1, $"[RMC GameSession] Migrate Session started for {client.User.Name}",Color.Blue);
                    Log.WriteLine(1, $"[RMC GameSession] Migrate Session from (id={client.GameSessionID}", Color.Red, client);
                    var reqMigrate = (RMCPacketRequestGameSessionService_MigrateSession)rmc.request;
                    migratefrom = Global.Sessions.Find(session => session.Key.SessionId == client.GameSessionID);
                    sesId = Global.NextGameSessionId++;
                    newsession = new Session(sesId, migratefrom.GameSession, client);
                    client.GameSessionID = sesId;
                    client.InGameSession = true;
                    if (migratefrom == null)
                    {
                        Log.WriteLine(1, $"[RMC GameSession] Migrate Session cannot find session to migrate from");
                    }
                    gameType = newsession.GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameType);
                    if (gameType == null)
                        Log.WriteLine(1, $"[Session] Inconsistent session state (id={newsession.Key.SessionId}), missing game type", Color.Red, client);
                    currPublicSlots = new Property() { Id = (uint)SessionParam.CurrentPublicSlots, Value = 0 };
                    currPrivateSlots = new Property() { Id = (uint)SessionParam.CurrentPrivateSlots, Value = 0 };
                    accessibility = new Property() { Id = (uint)SessionParam.Accessibility, Value = 0 };
                    Global.Sessions.Add(newsession);
                    // TODO: select new session, for now return the old key
                    reply = new RMCPacketResponseGameSessionService_MigrateSession(newsession.Key);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, "An error occurred: " + ex.Message, Color.Red);
                        Log.WriteLine(1, "Stack Trace:\n" + ex.StackTrace, Color.Red);
                    }
                    break;
                    //todo 
                    //Log.WriteLine(1, $"[RMC GameSession] Migrate Session migratedkey rcvd to {((RMCPacketResponseGameSessionService_MigrateSession)reply).MigratedKey.SessionId}",Color.Red);
                    //Log.WriteLine(1, $"[RMC GameSession] Migrate Session from (id={client.GameSessionID}", Color.Blue, client);
                    //Session migrateto = Global.Sessions.Find(session => session.Key.SessionId == ((RMCPacketResponseGameSessionService_MigrateSession)reply).MigratedKey.SessionId);
                    //if (client.InGameSession==false)
                    //{
                    //    Log.WriteLine(1, $"[RMC GameSession] Client not in session");
                    //    client.GameSessionID = migrateto.Key.SessionId;
                    //    client.InGameSession = true;
                    //}
                    //else if (migratefrom==null )
                    //{
                    //    Log.WriteLine(1, $"[RMC GameSession] Migrate Session cannot find session to migrate from");
                    //}
                    //if (migrateto.GameSession == null)
                    //{
                    //    Log.WriteLine(1, $"[RMC GameSession] Session not existant");
                    //    Log.WriteLine(1, $"[RMC GameSession] Migrate Session (id={migrateto})");
                    //}
                    //if (migrateto!=null && migratefrom!=null)
                    //{
                    //    Log.WriteLine(1, $"[RMC GameSession] Migrate Session (id={((RMCPacketResponseGameSessionService_MigrateSession)reply).MigratedKey.SessionId}) from {migratefrom.Key.SessionId}", Color.Blue, client);
                    //    client.GameSessionID = migrateto.Key.SessionId ;
                    //}
                    //
                    //RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    //break;
                case 5:
                    Log.WriteLine(1, $"[RMC GameSession] Leave Session");
                    var reqLeaveSes = (RMCPacketRequestGameSessionService_LeaveSession)rmc.request;
                    var leftSes = Global.Sessions.Find(session => session.Key.SessionId == reqLeaveSes.Key.SessionId);
                    // delete the session if empty
                    if (leftSes.NbParticipants() == 1)
                    {
                        Global.Sessions.Remove(leftSes);
                        Log.WriteLine(1, $"[RMC GameSession] Session {reqLeaveSes.Key.SessionId} deleted on leave from player {client.User.UserDBPid}", Color.Gray, client);
                    }
                    else
                        leftSes.Leave(client);
                    reply = new RMCPResponseEmpty();
                    client.GameSessionID = 0xffffffff;
                    client.InGameSession = false;
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 6:
                    var reqGetSession = (RMCPacketRequestGameSessionService_GetSession)rmc.request;
                    Log.WriteLine(1, $"[RMC GameSession] Get Session {reqGetSession.ToString()}");
                    reply = new RMCPacketResponseGameSessionService_GetSession(reqGetSession.Key, client);
                    
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
                    Log.WriteLine(1, $"[RMC] Addparticipants pids results: {string.Join(", ", reqAddParticip.PublicPids)} to session {client.GameSessionID}", Color.Green, client);
                    Global.Sessions.Find(session => session.Key.SessionId == reqAddParticip.Key.SessionId)
                        .AddParticipants(reqAddParticip.PublicPids, reqAddParticip.PrivatePids);
                    foreach (uint i in reqAddParticip.PublicPids)
                    {
                        ClientInfo result = Global.Clients.Find(cli => cli.ServerIncrementedGeneratedPID == i);
                        if (result!=null)
                        {
                            Log.WriteLine(1, $"[RMC] Addparticipants2 abandon: {result.User.UserDBPid} session {result.GameSessionID}", Color.Green, client);
                            if (result.InGameSession == true)
                            {
                               var future_abandoned  = Global.Sessions.Find(session => session.Key.SessionId == result.GameSessionID);
                                result.toabandon = true;
                                result.abandonID = result.GameSessionID;
                            }
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
                        ClientInfo result = Global.Clients.Find(cli => cli.ServerIncrementedGeneratedPID == i);
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
                    Log.WriteLine(1, $"[RMC] participants pids results: {reqRemoveParticip}", Color.Green, client);
                    Global.Sessions.Find(session => session.Key.SessionId == reqRemoveParticip.Key.SessionId)
                        .RemoveParticipants(reqRemoveParticip.PublicPids);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 12:
                    var sendInvitation = (RMCPacketRequestGameSessionService_SendInvitation)rmc.request;
                    Log.WriteLine(1, $"[RMC] GameSessionService SendInvitation \n${sendInvitation.Invitation.ToString()}", Color.Blue);
                    List<User> invitedUsers = DBHelper.GetUsersByID(sendInvitation.Invitation.Recipients);
                    Log.WriteLine(1, $"[RMC] GameSessionService SendInvitation \n${invitedUsers.Count}", Color.Blue);
                    Log.WriteLine(1, $"[RMC] GameSessionService SendInvitation \n${string.Join(",", sendInvitation.Invitation.Recipients)}", Color.Blue);
                    foreach (uint recipient in sendInvitation.Invitation.Recipients)
                    {
                        
                        ClientInfo tempClient = IsUserOnlinebyID(recipient);
                        Log.WriteLine(1, $"SendInvitation  to client {tempClient}", Color.Red);
                        if (tempClient != null )
                            {
                            var tempnotif = new NotificationQueueEntry(tempClient,
                                   1,
                                   0,
                                   7,
                                   5,
                                   0,
                                    sendInvitation.Invitation.Key.SessionId,
                                   sendInvitation.Invitation.Key.TypeId,
                                   sendInvitation.Invitation.Message);

                            Log.WriteLine(1, $"Sending notification {tempnotif.ToString()} ",Color.Red);
                            NotificationQueue.AddNotification(tempnotif);

                        }
                        else
                        {
                            Log.WriteLine(1, $" will send later notification about game invite");
                            Global.UIDNotificationQueue[recipient] = new OfflineNotificationEntry(0,7,5, sendInvitation.Invitation.Key.TypeId, sendInvitation.Invitation.Key.SessionId,0, sendInvitation.Invitation.Message);
                        }
                            }
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 14:
                    Log.WriteLine(1, $"[RMC] GameSessionService GetInvitationsReceived ",Color.Blue);
                    var reqGetInvRecv = (RMCPacketRequestGameSessionService_GetInvitationsReceived)rmc.request;
                    reply = new RMCPacketResponseGameSessionService_GetInvitationsReceived();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 17:
                    var InvitationReceived = (RMCPacketRequestGameSessionService_AcceptInvitation)rmc.request; //this happens when u get invite and accept it
                    Log.WriteLine(1, $"[RMC] GameSessionService GetInvitationsAccept {InvitationReceived.ToString()} ", Color.Blue);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 18:
                    var DenyInvitation = (RMCPacketRequestGameSessionService_DenyInvitation)rmc.request; //this happens when u get invite and deny it
                    Log.WriteLine(1, $"[RMC] GameSessionService GetInvitationsDenied {DenyInvitation.ToString()}", Color.Blue);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 19:
                    Log.WriteLine(1, $"[RMC] GameSessionService Cancel Invitation ", Color.Blue); // this happens when the host exits the game
                    var CancelInvitation = (RMCPacketRequestGameSessionService_CancelInvitation)rmc.request;
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 21://
                    Log.WriteLine(1, $"[RMC] GameSessionService RegisterURLs", Color.Blue);
                    var reqRegUrls = (RMCPacketRequestGameSessionService_RegisterURLs)rmc.request;
                    reqRegUrls.RegisterUrls(client);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 23:
                    Session abandonedSes;
                    
                    var reqAbandon = (RMCPacketRequestGameSessionService_AbandonSession)rmc.request;
                    if (client.toabandon == true)
                    {
                        client.toabandon = false;
                        abandonedSes = Global.Sessions.Find(session => session.Key.SessionId == client.abandonID);
                    }
                    else
                    {
                        abandonedSes = Global.Sessions.Find(session => session.Key.SessionId == reqAbandon.Key.SessionId);
                    }
                    Log.WriteLine(1, $"[RMC] RemoveGameSessionService Abandon Session client pid {client.User.UserDBPid} abandonedSes {abandonedSes.Key.SessionId}", Color.Blue);
                    if (abandonedSes != null)
                    {
                        if (abandonedSes.NbParticipants() == 1)
                        {
                            Global.Sessions.Remove(abandonedSes);
                            Log.WriteLine(1, $"[RMC GameSession] Session {abandonedSes.Key.SessionId} deleted on abandon from player {client.User.UserDBPid}", Color.Gray, client);
                            client.GameSessionID = 0xffffffff;
                            client.InGameSession = false;
                        }
                        else
                        {
                            bool notExist = false;
                            if (abandonedSes.PublicPids.Contains(client.ServerIncrementedGeneratedPID))
                            {
                                abandonedSes.PublicPids.Remove(client.ServerIncrementedGeneratedPID);
                            }
                            else
                            {
                                notExist = true;
                            }
                            if (abandonedSes.PrivatePids.Contains(client.ServerIncrementedGeneratedPID))
                            {
                                abandonedSes.PrivatePids.Remove(client.ServerIncrementedGeneratedPID);
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
                        Log.WriteLine(1, $"[]{Global.Sessions}");
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
