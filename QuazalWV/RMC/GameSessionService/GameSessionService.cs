using System.Drawing;
using System.IO;
using System.Windows.Input;

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
                    Log.WriteLine(1, "[RMC] CreateSession props:\n" + rmc.request.PayloadToString(), Color.Blue);
                    break;
                case 2:
                    rmc.request = new RMCPacketRequestGameSessionService_UpdateSession(s);
                    Log.WriteLine(1, "[RMC] UpdateSession props:\n" + rmc.request.PayloadToString(), Color.Purple);
                    break;
                case 4:
                    //rmc.request = new RMCPacketRequestGameSessionService_MigrateSession(s);
                    Log.WriteLine(1, $"[RMC GameSession] Error: Unknown Method MigrateSession {rmc.PayLoadToString()}", Color.Red);
                    //Log.WriteLine(1, "[RMC] UpdateSession props:\n" + rmc.request.PayloadToString(), Color.Purple);
                    break;
                case 5:
                    rmc.request = new RMCPacketRequestGameSessionService_LeaveSession(s);
                    Log.WriteLine(1, "[RMC] LeaveSession key:\n" + rmc.request.PayloadToString(), Color.Purple);
                    break;
                case 7:
                    rmc.request = new RMCPacketRequestGameSessionService_SearchSessions(s);
                    Log.WriteLine(1, "[RMC] SearchSessions query props:\n" + rmc.request.PayloadToString(), Color.Orange);
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
                        Log.WriteLine(1, $"[Session] Inconsistent session state (id={ses.Key.SessionId}), missing game type", Color.Red);
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
                        Log.WriteLine(1, $"[Session] Inconsistent session state (id={ses.Key.SessionId}), missing NAT type", Color.Red);
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
                case 5:
                    var reqleaveSes = (RMCPacketRequestGameSessionService_LeaveSession)rmc.request;
                    Global.Sessions.Find(session => session.Key.SessionId == reqleaveSes.key.SessionId)
                        .GameSession=null;
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 7:
                    var reqSearchSes = (RMCPacketRequestGameSessionService_SearchSessions)rmc.request;
                    Log.WriteLine(1, $"[RMC] SearchSessions query: {reqSearchSes.Query}", Color.Green);
                    reply = new RMCPacketResponseGameSessionService_SearchSessions(reqSearchSes.Query);
                    Log.WriteLine(1, $"[RMC] SearchSessions results: {((RMCPacketResponseGameSessionService_SearchSessions)reply).Results.Count}", Color.Green);
                   
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 8:
                    var reqAddParticip = (RMCPacketRequestGameSessionService_AddParticipants)rmc.request;
                    Global.Sessions.Find(session => session.Key.SessionId == reqAddParticip.Key.SessionId)
                        .AddParticipants(reqAddParticip.PublicPids, reqAddParticip.PrivatePids);
                    reply = new RMCPResponseEmpty();
                    Log.WriteLine(1, $"[RMC] Addparticipant query public pids: {string.Join(", ",reqAddParticip.PublicPids)}", Color.Green);
                    Log.WriteLine(1, $"[RMC] Addparticipant query private pids: {string.Join(", ", reqAddParticip.PrivatePids)}", Color.Green);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 9:
                    var reqRemoveParticip = (RMCPacketRequestGameSessionService_RemoveParticipants)rmc.request;
                    Global.Sessions.Find(session => session.Key.SessionId == reqRemoveParticip.Key.SessionId)
                        .RemoveParticipants(reqRemoveParticip.PublicPids, reqRemoveParticip.PrivatePids);
                    reply = new RMCPResponseEmpty();
                    Log.WriteLine(1, $"[RMC] RemoveParticipant query public pids: {string.Join(", ", reqRemoveParticip.PublicPids)}", Color.Green);
                    Log.WriteLine(1, $"[RMC] RemoveParticipant query private pids: {string.Join(", ", reqRemoveParticip.PrivatePids)}", Color.Green);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 12:
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 14:
                    var getRecvInvites = (RMCPacketRequestGameSessionService_GetInvitationsReceived)rmc.request;
                    reply = new RMCPacketResponseGameSessionService_GetInvitationsReceived();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 19:
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 21:
                    ((RMCPacketRequestGameSessionService_RegisterURLs)rmc.request).registerURLs(client);
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
