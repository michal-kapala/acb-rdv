using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public class GameSessionService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestGameSessionService_CreateSession(s);
                    Log.WriteLine(1, "[RMC GameSession] CreateSession props:\n" + rmc.request.PayloadToString(), Color.Blue);
                    break;
                case 2:
                    rmc.request = new RMCPacketRequestGameSessionService_UpdateSession(s);
                    Log.WriteLine(1, "[RMC GameSession] UpdateSession props:\n" + rmc.request.PayloadToString(), Color.Purple);
                    break;
                case 4:
                    rmc.request = new RMCPacketRequestGameSessionService_MigrateSession(s);
                    break;
                case 5:
                    rmc.request = new RMCPacketRequestGameSessionService_LeaveSession(s);
                    Log.WriteLine(1, "[RMC GameSession] LeaveSession key:\n" + rmc.request.PayloadToString(), Color.Purple);
                    break;
                case 7:
                    rmc.request = new RMCPacketRequestGameSessionService_SearchSessions(s);
                    Log.WriteLine(1, "[RMC GameSession] SearchSessions query props:\n" + rmc.request.PayloadToString(), Color.Orange);
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
                    Log.WriteLine(1, $"[RMC GameSession] Error: Unknown Method {rmc.methodID}", Color.Red);
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
                    var ses = new Session(sesId, reqCreateSes.Session, client);
                    // initialize params
                    var gameType = ses.GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameType);
                    if (gameType == null)
                        Log.WriteLine(1, $"[Session] Inconsistent session state (id={ses.Key.SessionId}), missing game type", Color.Red, client);
                    var currPublicSlots = new Property() { Id = (uint)SessionParam.CurrentPublicSlots, Value = 0 };
                    var currPrivateSlots = new Property() { Id = (uint)SessionParam.CurrentPrivateSlots, Value = 0 };
                    var accessibility = new Property() {  
                        Id = (uint)SessionParam.Accessibility,
                        Value = gameType.Value == (uint)GameType.PRIVATE ? 0u : 1u
                    };
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
                    var reqMigrate = (RMCPacketRequestGameSessionService_MigrateSession)rmc.request;
                    // TODO: select new session, for now return the old key
                    reply = new RMCPacketResponseGameSessionService_MigrateSession(reqMigrate.Key);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 5:
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
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 7:
                    var reqSearchSes = (RMCPacketRequestGameSessionService_SearchSessions)rmc.request;
                    Log.WriteLine(2, $"[RMC] SearchSessions query: {reqSearchSes.Query}", Color.Green, client);
                    reply = new RMCPacketResponseGameSessionService_SearchSessions(reqSearchSes.Query);
                    Log.WriteLine(2, $"[RMC] SearchSessions results: {((RMCPacketResponseGameSessionService_SearchSessions)reply).Results.Count}", Color.Green, client);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 8:
                    var reqAddParticip = (RMCPacketRequestGameSessionService_AddParticipants)rmc.request;
                    Global.Sessions.Find(session => session.Key.SessionId == reqAddParticip.Key.SessionId)
                        .AddParticipants(reqAddParticip.PublicPids, reqAddParticip.PrivatePids);
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
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 14:
                    var reqGetInvRecv = (RMCPacketRequestGameSessionService_GetInvitationsReceived)rmc.request;
                    reply = new RMCPacketResponseGameSessionService_GetInvitationsReceived();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
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
                    var abandonedSes = Global.Sessions.Find(session => session.Key.SessionId == reqAbandon.Key.SessionId);
                    if (abandonedSes != null)
                    {
                        abandonedSes.PrivatePids.Remove(client.PID);
                        if (abandonedSes.PrivatePids.Count == 0)
                            Global.Sessions.Remove(abandonedSes);
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
