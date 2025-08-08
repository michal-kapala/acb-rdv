using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace QuazalWV
{
    public class GameSessionService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.GameSession;

        public static void ProcessRequest(Stream s, RMCP rmc, ClientInfo client)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestGameSessionService_CreateSession(s);
                    Log.WriteRmcLine(1, "CreateSession props:\n" + rmc.request.PayloadToString(), protocol, LogSource.RMC, Color.Blue, client);
                    break;
                case 2:
                    rmc.request = new RMCPacketRequestGameSessionService_UpdateSession(s);
                    Log.WriteRmcLine(1, "UpdateSession props:\n" + rmc.request.PayloadToString(), protocol, LogSource.RMC, Color.Purple, client);
                    break;
                case 4:
                    rmc.request = new RMCPacketRequestGameSessionService_MigrateSession(s);
                    break;
                case 5:
                    rmc.request = new RMCPacketRequestGameSessionService_LeaveSession(s);
                    break;
                case 6:
                    rmc.request = new RMCPacketRequestGameSessionService_GetSession(s);
                    break;
                case 7:
                    rmc.request = new RMCPacketRequestGameSessionService_SearchSessions(s);
                    Log.WriteRmcLine(1, "SearchSessions query props:\n" + rmc.request.PayloadToString(), protocol, LogSource.RMC, Color.Orange, client);
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
                    rmc.request = new RMCPacketRequestGameSessionService_DeclineInvitation(s);
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
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }

        public static void HandleRequest(PrudpPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            uint sesId;
            Property gameType, currPublicSlots, currPrivateSlots, accessibility;
            Session newSes, migrateFromSes;
            ClientInfo inviter;
            switch (rmc.methodID)
            {
                case 1:
                    var reqCreateSes = (RMCPacketRequestGameSessionService_CreateSession)rmc.request;
                    sesId = Global.NextGameSessionId++;
                    client.GameSessionID = sesId;
                    newSes = new Session(sesId, reqCreateSes.Session, client);
                    // initialize params
                    gameType = newSes.GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameType);
                    if (gameType == null)
                        Log.WriteLine(1, $"Inconsistent session state (id={newSes.Key.SessionId}), missing game type", LogSource.Session, Color.Red, client);
                    currPublicSlots = new Property() { Id = (uint)SessionParam.CurrentPublicSlots, Value = 0 };
                    currPrivateSlots = new Property() { Id = (uint)SessionParam.CurrentPrivateSlots, Value = 0 };
                    accessibility = new Property() { Id = (uint)SessionParam.Accessibility, Value = 0 };
                    newSes.GameSession.Attributes.Add(currPublicSlots);
                    newSes.GameSession.Attributes.Add(currPrivateSlots);
                    newSes.GameSession.Attributes.Add(accessibility);
                    // blind NAT type update
                    var natType = newSes.GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.NatType);
                    if (natType == null)
                        Log.WriteLine(1, $"Inconsistent session state (id={newSes.Key.SessionId}), missing NAT type", LogSource.Session, Color.Red, client);
                    natType.Value = (uint)NatType.OPEN;
                    Global.Sessions.Add(newSes);
                    reply = new RMCPacketResponseGameSessionService_CreateSession(reqCreateSes.Session.TypeId, sesId);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 2:
                    var reqUpdateSes = (RMCPacketRequestGameSessionService_UpdateSession)rmc.request;
                    newSes = Global.Sessions.Find(session => session.Key.SessionId == reqUpdateSes.SessionUpdate.Key.SessionId);
                    if (newSes == null)
                        Log.WriteRmcLine(1, $"Update for deleted session {reqUpdateSes.SessionUpdate.Key.SessionId}", protocol, LogSource.RMC, Color.Red, client);
                    foreach (var newParam in reqUpdateSes.SessionUpdate.Attributes)
                    {
                        var existing = newSes.GameSession.Attributes.FirstOrDefault(attr => attr.Id == newParam.Id);
                        if (existing != null)
                            existing.Value = newParam.Value;
                        else
                            newSes.GameSession.Attributes.Add(newParam);
                    }
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 4:
                    var reqMigrate = (RMCPacketRequestGameSessionService_MigrateSession)rmc.request;
                    try
                    {
                        Log.WriteRmcLine(1, $"Migrating from session {reqMigrate.Key.SessionId}", protocol, LogSource.RMC, Color.Blue, client);
                        migrateFromSes = Global.Sessions.Find(session => session.Key.SessionId == reqMigrate.Key.SessionId);
                        if (migrateFromSes == null)
                        {
                            Log.WriteRmcLine(1, $"Migrating session not found", protocol, LogSource.RMC, Color.Red, client);
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_InvalidSessionKey);
                        }
                        else
                        {
                            migrateFromSes.Migrating = true;
                            reply = new RMCPacketResponseGameSessionService_MigrateSession(reqMigrate.Key);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteRmcLine(1, $"MigrateSession: {ex.Message}", protocol, LogSource.RMC, Color.Red, client);
                    }
                    break;
                case 5:
                    // does not change the session state
                    var reqLeaveSes = (RMCPacketRequestGameSessionService_LeaveSession)rmc.request;
                    reply = new RMCPResponseEmpty();
                    client.GameSessionID = 0;
                    client.InGameSession = false;
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 6:
                    var reqGetSes = (RMCPacketRequestGameSessionService_GetSession)rmc.request;
                    newSes = Global.Sessions.Find(session => session.Key.SessionId == reqGetSes.Key.SessionId);
                    if (newSes == null)
                    {
                        Log.WriteRmcLine(1, $"Session {reqGetSes.Key.SessionId} not found", protocol, LogSource.RMC, Color.Red, client);
                        reply = new RMCPResponseEmpty();
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_InvalidSessionKey);
                    }
                    else if (newSes.IsJoinable())
                    {
                        ClientInfo host = Global.Clients.Find(c => c.User.Pid == newSes.HostPid);
                        if (host == null)
                        {
                            Log.WriteRmcLine(1, $"Session host {newSes.HostPid} not found", protocol, LogSource.RMC, Color.Red, client);
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_InvalidPID);
                        }
                        else
                        {
                            reply = new RMCPacketResponseGameSessionService_GetSession(newSes, host);
                            Log.WriteRmcLine(1, $"Session {reqGetSes.Key.SessionId} found", protocol, LogSource.RMC, Color.Blue, client);
                            foreach (var url in ((RMCPacketResponseGameSessionService_GetSession)reply).SearchResult.HostUrls)
                                Log.WriteLine(1, $"[{url}]", LogSource.StationURL, Color.Blue, client);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                        }
                    }
                    else
                    {
                        reply = new RMCPResponseEmpty();
                        var isPrivateSes = newSes.FindProp(SessionParam.IsPrivate);
                        if (isPrivateSes == null)
                        {
                            Log.WriteRmcLine(1, $"Session {reqGetSes.Key.SessionId} missing IsPrivate param", protocol, LogSource.RMC, Color.Red, client);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_Unknown);
                        }
                        else
                        {
                            Log.WriteRmcLine(1, $"Session {reqGetSes.Key.SessionId} is full", protocol, LogSource.RMC, Color.Red, client);
                            QError error = isPrivateSes.Value == 0 ? QError.GameSession_NoPublicSlotLeft : QError.GameSession_NoPrivateSlotLeft;
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)error);
                        }
                    }
                    break;
                case 7:
                    var reqSearchSes = (RMCPacketRequestGameSessionService_SearchSessions)rmc.request;
                    Log.WriteRmcLine(2, $"SearchSessions query: {reqSearchSes.Query}", protocol, LogSource.RMC, Color.Green, client);
                    reply = new RMCPacketResponseGameSessionService_SearchSessions(reqSearchSes.Query, client);
                    Log.WriteRmcLine(2, $"SearchSessions results: {((RMCPacketResponseGameSessionService_SearchSessions)reply).Results.Count}", protocol, LogSource.RMC, Color.Green, client);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 8:
                    var reqAddParticip = (RMCPacketRequestGameSessionService_AddParticipants)rmc.request;
                    // update session
                    Global.Sessions.Find(session => session.Key.SessionId == reqAddParticip.Key.SessionId)
                        .AddParticipants(reqAddParticip.PublicPids, reqAddParticip.PrivatePids);
                    // update clients
                    foreach (uint pid in reqAddParticip.PublicPids)
                    {
                        ClientInfo result = Global.Clients.Find(c => c.User.Pid == pid);
                        if (result != null)
                        {
                            if (result.InGameSession == true)
                            {
                                Log.WriteRmcLine(1, $"{result.User.Name} already in session {result.GameSessionID} on AddParticipants for session {reqAddParticip.Key.SessionId}", protocol, LogSource.RMC, Color.Orange, client);
                                var future_abandoned  = Global.Sessions.Find(session => session.Key.SessionId == result.GameSessionID);
                                result.AbandoningSession = true;
                                result.AbandonedSessionID = result.GameSessionID;
                            }
                            result.GameSessionID = reqAddParticip.Key.SessionId;
                            result.InGameSession = true;
                        }
                        else
                            Log.WriteRmcLine(1, $"AddParticipants: player {pid} is not online", protocol, LogSource.RMC, Color.Red, client);
                    }

                    foreach (uint pid in reqAddParticip.PrivatePids)
                    {
                        ClientInfo result = Global.Clients.Find(c => c.User.Pid == pid);
                        if (result != null)
                        {
                            if (result.InGameSession == true)
                            {
                                Log.WriteRmcLine(1, $"{result.User.Name} already in session {result.GameSessionID} on AddParticipants for session {reqAddParticip.Key.SessionId}", protocol, LogSource.RMC, Color.Orange, client);
                                var future_abandoned = Global.Sessions.Find(session => session.Key.SessionId == result.GameSessionID);
                                result.AbandoningSession = true;
                                result.AbandonedSessionID = result.GameSessionID;
                            }
                            result.GameSessionID = reqAddParticip.Key.SessionId;
                            result.InGameSession = true;
                        }
                        else
                            Log.WriteRmcLine(1, $"AddParticipants: player {pid} is not online", protocol, LogSource.RMC, Color.Red, client);
                    }
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 9:
                    var reqRemoveParticip = (RMCPacketRequestGameSessionService_RemoveParticipants)rmc.request;
                    Global.Sessions.Find(session => session.Key.SessionId == reqRemoveParticip.Key.SessionId)
                        .RemoveParticipants(reqRemoveParticip.Pids);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 12:
                    var reqSendInvitation = (RMCPacketRequestGameSessionService_SendInvitation)rmc.request;
                    Log.WriteRmcLine(1, $"SendInvitation:\n{reqSendInvitation.Invitation}", protocol, LogSource.RMC, Color.Blue, client);
                    ClientInfo invitee;
                    foreach (uint pid in reqSendInvitation.Invitation.Recipients)
                    {
                        invitee = Global.Clients.Find(c => c.User.Pid == pid);
                        if (invitee != null)
                            NotificationManager.GameInviteSent(invitee, client.User.Pid, reqSendInvitation.Invitation);
                        else
                            DbHelper.AddGameInvites(reqSendInvitation.Invitation.Key, client.User.Pid, pid, reqSendInvitation.Invitation.Message);
                    }
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 14:
                    var reqGetInvRecv = (RMCPacketRequestGameSessionService_GetInvitationsReceived)rmc.request;
                    reply = new RMCPacketResponseGameSessionService_GetInvitationsReceived();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 17:
                    var reqAcceptInvite = (RMCPacketRequestGameSessionService_AcceptInvitation)rmc.request;
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    // invite accepted notif
                    inviter = Global.Clients.Find(c => c.User.Pid == reqAcceptInvite.InvitationRecv.SenderPid);
                    if (inviter != null)
                        NotificationManager.GameInviteAccepted(inviter, client.User.Pid, reqAcceptInvite.InvitationRecv.SessionKey.SessionId);
                    break;
                case 18:
                    var reqDeclineInvite = (RMCPacketRequestGameSessionService_DeclineInvitation)rmc.request;
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    // invite declined notif
                    inviter = Global.Clients.Find(c => c.User.Pid == reqDeclineInvite.InvitationRecv.SenderPid);
                    if (inviter != null)
                        NotificationManager.GameInviteDeclined(inviter, client.User.Pid, reqDeclineInvite.InvitationRecv.SessionKey.SessionId);
                    break;
                case 19:
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 21:
                    var reqRegUrls = (RMCPacketRequestGameSessionService_RegisterURLs)rmc.request;
                    reply = new RMCPResponseEmpty();
                    if (client.GameSessionID == 0)
                    {
                        Log.WriteRmcLine(1, $"RegisterURLs: {client.User.Name} is not in a session", protocol, LogSource.RMC, Color.Red, client);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_PlayerIsNotSessionParticipant);
                    }
                    else
                    {
                        var ses = Global.Sessions.Find(s => s.Key.SessionId == client.GameSessionID);
                        if (ses == null)
                        {
                            Log.WriteRmcLine(1, $"RegisterURLs: session {client.GameSessionID} was deleted", protocol, LogSource.RMC, Color.Red, client);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_Unknown);
                        }
                        else
                        {
                            var newHostPid = reqRegUrls.Urls.First().PID;
                            ses.HostPid = newHostPid;
                            reqRegUrls.RegisterUrls(client, ses);
                            if (ses.Migrating)
                            {
                                Log.WriteRmcLine(1, $"RegisterURLs: Host migration for session {ses.Key.SessionId}, new host {newHostPid}", protocol, LogSource.RMC, Color.Blue, client);
                                ses.Migrating = false;
                            }
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                        }
                    }
                    break;
                case 23:
                    var reqAbandon = (RMCPacketRequestGameSessionService_AbandonSession)rmc.request;
                    Session abandonedSes;
                    if (client.AbandoningSession == true)
                    {
                        client.AbandoningSession = false;
                        abandonedSes = Global.Sessions.Find(session => session.Key.SessionId == client.AbandonedSessionID);
                    }
                    else
                        abandonedSes = Global.Sessions.Find(session => session.Key.SessionId == reqAbandon.Key.SessionId);
                    if (abandonedSes != null)
                    {
                        client.GameSessionID = 0;
                        client.InGameSession = false;
                        bool removed = false;
                        if (abandonedSes.PublicPids.Contains(client.User.Pid))
                            removed = abandonedSes.PublicPids.Remove(client.User.Pid);

                        if (abandonedSes.PrivatePids.Contains(client.User.Pid))
                        {
                            bool privRemoved = abandonedSes.PrivatePids.Remove(client.User.Pid);
                            removed = removed ? removed : privRemoved;
                        }
                        
                        if (abandonedSes.NbParticipants() == 0)
                        {
                            // duplicate request check
                            if (removed)
                            {
                                Global.Sessions.Remove(abandonedSes);
                                Log.WriteRmcLine(1, $"Session {abandonedSes.Key.SessionId} deleted on abandon from player {client.User.Pid}", protocol, LogSource.RMC, Color.Gray, client);
                            }
                            else
                                Log.WriteRmcLine(1, $"AbandonSession request duplicate", protocol, LogSource.RMC, Color.Gray, client);
                        }
                    }
                    else
                        Log.WriteRmcLine(1, $"AbandonSession: session {reqAbandon.Key.SessionId} not found", protocol, LogSource.RMC, Color.Red, client);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }
    }
}
