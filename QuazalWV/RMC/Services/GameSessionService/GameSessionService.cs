using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
                    Log.WriteRmcLine(1, "CreateSession props:\n" + rmc.request.PayloadToString(), protocol, LogSource.RMC, Global.DarkTheme ? Color.RoyalBlue : Color.Blue, client);
                    break;
                case 2:
                    rmc.request = new RMCPacketRequestGameSessionService_UpdateSession(s);
                    Log.WriteRmcLine(1, "UpdateSession props:\n" + rmc.request.PayloadToString(), protocol, LogSource.RMC, Global.DarkTheme ? Color.Violet : Color.Purple, client);
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
                    rmc.request = new RMCPacketRequestGameSessionService_RegisterURLs(s, client);
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
                    // Check if private sessions are allowed (excluding intro sessions)
                    var gameModeProp = reqCreateSes.Session.Attributes.Find(param => param.Id == (uint)SessionParam.GameMode);
                    var gameTypeProp = reqCreateSes.Session.Attributes.Find(param => param.Id == (uint)SessionParam.GameType);
                    if ((GameMode)gameModeProp.Value != GameMode.Intro && (GameType)gameTypeProp.Value == GameType.PRIVATE && !Global.AllowPrivateSessions)
                    {
                        Log.WriteLine(1, $"Private sessions are permitted - creation denied for client {client.User.Pid}", LogSource.Session, Color.OrangeRed, client);
                        reply = new RMCPResponseEmpty();
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_Unknown);
                        break;
                    }
                    // Check if the client already has a session and remove it
                    var oldSession = Global.Sessions.FirstOrDefault(s => s.HostPid == client.User.Pid);
                    if (oldSession != null)
                    {
                        Log.WriteLine(1, $"Removing old session {oldSession.Key.SessionId} for host {client.User.Pid}", LogSource.Session, Color.Orange);
                        Global.Sessions.Remove(oldSession);
                    }
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
                    {
                        Log.WriteRmcLine(1,
                            $"UpdateSession requested for deleted/non-existent session {reqUpdateSes.SessionUpdate.Key.SessionId}", protocol, LogSource.RMC, Color.Red, client);
                        reply = new RMCPResponseEmpty();
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_InvalidSessionKey);
                        break;
                    }
                    try
                    {
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
                    }
                    catch (Exception ex)
                    {
                        Log.WriteRmcLine(1, $"UpdateSession exception: {ex}", protocol, LogSource.RMC, Color.Red, client);

                        reply = new RMCPResponseEmpty();
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_Unknown);
                    }
                    break;
                case 4:
                    var reqMigrate = (RMCPacketRequestGameSessionService_MigrateSession)rmc.request;
                    try
                    {
                        Log.WriteRmcLine(1, $"Migrating from session {reqMigrate.Key.SessionId}", protocol, LogSource.RMC, Global.DarkTheme ? Color.RoyalBlue : Color.Blue, client);
                        migrateFromSes = Global.Sessions.Find(session => session.Key.SessionId == reqMigrate.Key.SessionId);
                        if (migrateFromSes == null)
                        {
                            Log.WriteRmcLine(1, $"Migrating session not found", protocol, LogSource.RMC, Color.Red, client);
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_InvalidSessionKey);
                        }
                        else
                        {
                            // Mark session as migrating
                            migrateFromSes.Migrating = true;

                            // Remember the previous host PID / client
                            var prevHostPid = migrateFromSes.HostPid;

                            // Set client to the session immediately
                            client.GameSessionID = migrateFromSes.Key.SessionId;
                            client.InGameSession = true;

                            // Determine new host
                            if (client.ep?.Port == 7917)
                            {
                                // Client has a reliable connection, make them host
                                migrateFromSes.HostPid = client.User.Pid;
                                Log.WriteRmcLine(1, $"Client {client.User.Pid} has a reliable connection and became the new host", protocol, LogSource.RMC, Color.Orange, client);
                            }
                            else
                            {
                                // Try to find another valid host in the session (exclude previous host)
                                var newHostCandidate = Global.Clients
                                    .FirstOrDefault(c =>
                                        c.User != null &&
                                        c.ep?.Port == 7917 &&
                                        c.User.Pid != prevHostPid &&
                                        (migrateFromSes.PublicPids.Contains(c.User.Pid) || migrateFromSes.PrivatePids.Contains(c.User.Pid)));

                                if (newHostCandidate != null)
                                {
                                    migrateFromSes.HostPid = newHostCandidate.User.Pid;
                                    Log.WriteRmcLine(1, $"Client {client.User.Pid} shouldn't be host, elected a more reliable client {newHostCandidate.User.Pid} instead", protocol, LogSource.RMC, Color.Orange, client);
                                }
                                else
                                {
                                    // Fallback: pick any client in the session (exclude previous host)
                                    var fallbackHost = Global.Clients
                                        .FirstOrDefault(c =>
                                            c.User != null &&
                                            c.User.Pid != prevHostPid &&
                                            (migrateFromSes.PublicPids.Contains(c.User.Pid) || migrateFromSes.PrivatePids.Contains(c.User.Pid)));

                                    if (fallbackHost != null)
                                    {
                                        migrateFromSes.HostPid = fallbackHost.User.Pid;
                                        Log.WriteRmcLine(1, $"No other reliable host found, client {fallbackHost.User.Pid} will become the new host as a fallback", protocol, LogSource.RMC, Color.Orange, client);
                                    }
                                    else
                                    {
                                        Log.WriteRmcLine(1, $"No clients available to host session {migrateFromSes.Key.SessionId}, session remains in migrating state", protocol, LogSource.RMC, Color.Red, client);
                                    }
                                }
                            }

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
                    client.PreSessionSearchCount = 0;
                    client.PlayNowQuery = false;
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
                            Log.WriteRmcLine(1, $"Session {reqGetSes.Key.SessionId} found", protocol, LogSource.RMC, Global.DarkTheme ? Color.RoyalBlue : Color.Blue, client);
                            foreach (var url in ((RMCPacketResponseGameSessionService_GetSession)reply).SearchResult.HostUrls)
                                Log.WriteLine(1, $"[{url}]", LogSource.StationURL, Global.DarkTheme ? Color.RoyalBlue : Color.Blue, client);
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
                    if (!client.InGameSession)
                        client.PreSessionSearchCount++;
                    if (client.PreSessionSearchCount > 2)
                        client.PlayNowQuery = true;
                    Log.WriteRmcLine(2, $"SearchSessions query: {reqSearchSes.Query}", protocol, LogSource.RMC, Global.DarkTheme ? Color.LimeGreen : Color.Green, client);
                    reply = new RMCPacketResponseGameSessionService_SearchSessions(reqSearchSes.Query, client);
                    Log.WriteRmcLine(2, $"SearchSessions results: {((RMCPacketResponseGameSessionService_SearchSessions)reply).Results.Count}", protocol, LogSource.RMC, Global.DarkTheme ? Color.LimeGreen : Color.Green, client);
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
                    Log.WriteRmcLine(1, $"SendInvitation:\n{reqSendInvitation.Invitation}", protocol, LogSource.RMC, Global.DarkTheme ? Color.RoyalBlue : Color.Blue, client);
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
                    {
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
                                client.GameSessionID = 0;
                                client.InGameSession = false;
                                RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.GameSession_Unknown);
                            }
                            else
                            {
                                // Only update HostPid and HostUrls if this client is the session host
                                if (client.User.Pid == ses.HostPid)
                                {
                                    // Update host URLs from client
                                    ses.HostUrls = new List<StationUrl>(reqRegUrls.Urls);

                                    // For WAN communication add a separate NAT URL
                                    if (!Global.IsPrivate)
                                    {
                                        var NATUrl = new StationUrl(client);
                                        NATUrl.Address = client.ep.Address.ToString();
                                        reqRegUrls.Urls.Add(NATUrl);
                                        Log.WriteRmcLine(1, $"RegisterURLs - NAT URL added: {NATUrl}", protocol, LogSource.RMC, Color.Brown, client);
                                    }
                                }

                                // Register URLs
                                reqRegUrls.RegisterUrls(client, ses);

                                if (ses.Migrating)
                                {
                                    Log.WriteRmcLine(1, $"RegisterURLs: Host migration for session {ses.Key.SessionId}, new host {ses.HostPid}", protocol, LogSource.RMC, Global.DarkTheme ? Color.RoyalBlue : Color.Blue, client);
                                    ses.Migrating = false;
                                }

                                RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                            }
                        }
                        break;
                    }
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
