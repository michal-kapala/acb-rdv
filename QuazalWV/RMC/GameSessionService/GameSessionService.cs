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
                    break;
                case 6:
                    rmc.request = new RMCPacketRequestGameSessionService_GetSession(s);
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
                    Log.WriteLine(1, $"[RMC GameSession] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
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
                        Log.WriteLine(1, $"[Session] Inconsistent session state (id={newSes.Key.SessionId}), missing game type", Color.Red, client);
                    currPublicSlots = new Property() { Id = (uint)SessionParam.CurrentPublicSlots, Value = 0 };
                    currPrivateSlots = new Property() { Id = (uint)SessionParam.CurrentPrivateSlots, Value = 0 };
                    accessibility = new Property() { Id = (uint)SessionParam.Accessibility, Value = 0 };
                    newSes.GameSession.Attributes.Add(currPublicSlots);
                    newSes.GameSession.Attributes.Add(currPrivateSlots);
                    newSes.GameSession.Attributes.Add(accessibility);
                    // blind NAT type update
                    var natType = newSes.GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.NatType);
                    if (natType == null)
                        Log.WriteLine(1, $"[Session] Inconsistent session state (id={newSes.Key.SessionId}), missing NAT type", Color.Red, client);
                    natType.Value = (uint)NatType.OPEN;
                    Global.Sessions.Add(newSes);
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
                    var reqMigrate = (RMCPacketRequestGameSessionService_MigrateSession)rmc.request;
                    try
                    {
                        Log.WriteLine(1, $"[RMC GameSession] Migrating from session {reqMigrate.Key.SessionId}", Color.Blue, client);
                        migrateFromSes = Global.Sessions.Find(session => session.Key.SessionId == reqMigrate.Key.SessionId);
                        sesId = Global.NextGameSessionId++;
                        newSes = new Session(sesId, migrateFromSes.GameSession, client);
                        client.GameSessionID = sesId;
                        client.InGameSession = true;
                        if (migrateFromSes == null)
                            Log.WriteLine(1, $"[RMC GameSession] Current session not found", Color.Red, client);
                        gameType = newSes.GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameType);
                        if (gameType == null)
                            Log.WriteLine(1, $"[RMC GameSession] Inconsistent session state (id={newSes.Key.SessionId}), missing game type", Color.Red, client);
                        currPublicSlots = new Property() { Id = (uint)SessionParam.CurrentPublicSlots, Value = 0 };
                        currPrivateSlots = new Property() { Id = (uint)SessionParam.CurrentPrivateSlots, Value = 0 };
                        accessibility = new Property() { Id = (uint)SessionParam.Accessibility, Value = 0 };
                        Global.Sessions.Add(newSes);
                        // TODO: check if the target session should be new or existing + if old should be removed
                        reply = new RMCPacketResponseGameSessionService_MigrateSession(newSes.Key);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, $"[RMC GameSession] MigrateSession: {ex.Message}", Color.Red, client);
                    }
                    break;
                case 5:
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
                    client.GameSessionID = 0;
                    client.InGameSession = false;
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 6:
                    var reqGetSes = (RMCPacketRequestGameSessionService_GetSession)rmc.request;
                    newSes = Global.Sessions.Find(session => session.Key.SessionId == reqGetSes.Key.SessionId);
                    if (newSes == null)
                    {
                        Log.WriteLine(1, $"[RMC GameSession] Session {reqGetSes.Key.SessionId} not found", Color.Red, client);
                        reply = new RMCPResponseEmpty();
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_InvalidSessionKey);
                    }
                    else if (newSes.IsJoinable())
                    {
                        ClientInfo host = Global.Clients.Find(c => c.User.UserDBPid == newSes.HostPid);
                        if (host == null)
                        {
                            Log.WriteLine(1, $"[RMC GameSession] Session host {newSes.HostPid} not found", Color.Red, client);
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_InvalidPID);
                        }
                        else
                        {
                            reply = new RMCPacketResponseGameSessionService_GetSession(newSes, host);
                            Log.WriteLine(1, $"[RMC GameSession] Session {reqGetSes.Key.SessionId} found", Color.Blue, client);
                            foreach (var url in ((RMCPacketResponseGameSessionService_GetSession)reply).SearchResult.HostUrls)
                                Log.WriteLine(1, $"[{url}]", Color.Blue, client);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                        }
                    }
                    else
                    {
                        reply = new RMCPResponseEmpty();
                        var isPrivateSes = newSes.FindProp(SessionParam.IsPrivate);
                        if (isPrivateSes == null)
                        {
                            Log.WriteLine(1, $"[RMC GameSession] Session {reqGetSes.Key.SessionId} missing IsPrivate param", Color.Red, client);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_Unknown);
                        }
                        else
                        {
                            Log.WriteLine(1, $"[RMC GameSession] Session {reqGetSes.Key.SessionId} is full", Color.Red, client);
                            QError error = isPrivateSes.Value == 0 ? QError.GameSession_NoPublicSlotLeft : QError.GameSession_NoPrivateSlotLeft;
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)error);
                        }
                    }
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
                    // update session
                    Global.Sessions.Find(session => session.Key.SessionId == reqAddParticip.Key.SessionId)
                        .AddParticipants(reqAddParticip.PublicPids, reqAddParticip.PrivatePids);
                    // update clients
                    foreach (uint pid in reqAddParticip.PublicPids)
                    {
                        ClientInfo result = Global.Clients.Find(c => c.User.UserDBPid == pid);
                        if (result != null)
                        {
                            if (result.InGameSession == true)
                            {
                                Log.WriteLine(1, $"[RMC] {result.User.Name} already in session {result.GameSessionID} on AddParticipants for session {reqAddParticip.Key.SessionId}", Color.Orange, client);
                                var future_abandoned  = Global.Sessions.Find(session => session.Key.SessionId == result.GameSessionID);
                                result.AbandoningSession = true;
                                result.AbandonedSessionID = result.GameSessionID;
                                // TODO: update old session
                            }
                            result.GameSessionID = reqAddParticip.Key.SessionId;
                            result.InGameSession = true;
                        }
                        else
                            Log.WriteLine(1, $"[RMC GameSession] AddParticipants: player {pid} is not online", Color.Red, client);
                    }

                    foreach (uint pid in reqAddParticip.PrivatePids)
                    {
                        ClientInfo result = Global.Clients.Find(c => c.User.UserDBPid == pid);
                        if (result != null)
                        {
                            if (result.InGameSession == true)
                            {
                                Log.WriteLine(1, $"[RMC] {result.User.Name} already in session {result.GameSessionID} on AddParticipants for session {reqAddParticip.Key.SessionId}", Color.Orange, client);
                                var future_abandoned = Global.Sessions.Find(session => session.Key.SessionId == result.GameSessionID);
                                result.AbandoningSession = true;
                                result.AbandonedSessionID = result.GameSessionID;
                                // TODO: update old session
                            }
                            result.GameSessionID = reqAddParticip.Key.SessionId;
                            result.InGameSession = true;
                        }
                        else
                            Log.WriteLine(1, $"[RMC GameSession] AddParticipants: player {pid} is not online", Color.Red, client);
                    }
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 9:
                    var reqRemoveParticip = (RMCPacketRequestGameSessionService_RemoveParticipants)rmc.request;
                    Global.Sessions.Find(session => session.Key.SessionId == reqRemoveParticip.Key.SessionId)
                        .RemoveParticipants(reqRemoveParticip.PublicPids, reqRemoveParticip.PrivatePids);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 12:
                    var reqSendInvitation = (RMCPacketRequestGameSessionService_SendInvitation)rmc.request;
                    Log.WriteLine(1, $"[RMC GameSession] SendInvitation:\n{reqSendInvitation.Invitation}", Color.Blue, client);
                    ClientInfo invitee;
                    foreach (uint pid in reqSendInvitation.Invitation.Recipients)
                    {
                        invitee = Global.Clients.Find(c => c.User.UserDBPid == pid);
                        if (invitee != null)
                            NotificationManager.GameInviteSent(invitee, client.User.UserDBPid, reqSendInvitation.Invitation);
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
                    inviter = Global.Clients.Find(c => c.User.UserDBPid == reqAcceptInvite.InvitationRecv.SenderPid);
                    if (inviter != null)
                        NotificationManager.GameInviteAccepted(inviter, client.User.UserDBPid, reqAcceptInvite.InvitationRecv.SessionKey.SessionId);
                    break;
                case 18:
                    var reqDeclineInvite = (RMCPacketRequestGameSessionService_DeclineInvitation)rmc.request;
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    // invite declined notif
                    inviter = Global.Clients.Find(c => c.User.UserDBPid == reqDeclineInvite.InvitationRecv.SenderPid);
                    if (inviter != null)
                        NotificationManager.GameInviteDeclined(inviter, client.User.UserDBPid, reqDeclineInvite.InvitationRecv.SessionKey.SessionId);
                    break;
                case 19:
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 21:
                    var reqRegUrls = (RMCPacketRequestGameSessionService_RegisterURLs)rmc.request;
                    reqRegUrls.RegisterUrls(client);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
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
                        if (abandonedSes.NbParticipants() == 1)
                        {
                            Global.Sessions.Remove(abandonedSes);
                            Log.WriteLine(1, $"[RMC GameSession] Session {abandonedSes.Key.SessionId} deleted on abandon from player {client.User.UserDBPid}", Color.Gray, client);
                            client.GameSessionID = 0;
                            client.InGameSession = false;
                        }
                        else
                        {
                            if (abandonedSes.PublicPids.Contains(client.User.UserDBPid))
                                abandonedSes.PublicPids.Remove(client.User.UserDBPid);
                            if (abandonedSes.PrivatePids.Contains(client.User.UserDBPid))
                                abandonedSes.PrivatePids.Remove(client.User.UserDBPid);
                        }
                    }
                    else
                    {
                        Log.WriteLine(1, $"[RMC GameSession] AbandonSession: session {reqAbandon.Key.SessionId} not found", Color.Red, client);
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
