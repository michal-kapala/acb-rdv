using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace QuazalWV
{
    public static class ContactsService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.ContactsExtensions;

        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 3:
                    rmc.request = new RMCPacketRequestContactsService_RetrieveGameSessionFromContact(s);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red);
                    break;
            }
        }

        public static void HandleRequest(PrudpPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 3:
                    var reqGetFriendSessions = (RMCPacketRequestContactsService_RetrieveGameSessionFromContact)rmc.request;
                    List<GameSessionByPlayerInfo> gameSessions = new List<GameSessionByPlayerInfo>();
                    foreach (string name in reqGetFriendSessions.FriendNames)
                    {
                        Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: requesting {name}'s session", protocol, LogSource.RMC, Global.DarkTheme ? Color.LimeGreen : Color.Green, client);
                        ClientInfo friend = Global.Clients.FirstOrDefault(c => c.User?.Name == name);
                        if (friend == null)
                        {
                            Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: for {name} who's not currently online.", protocol, LogSource.RMC, Color.Red, client);
                            continue;
                        }

                        // Find or recover the session
                        Session session = Global.Sessions.FirstOrDefault(s => s.Key.SessionId == friend.GameSessionID)
                                   ?? Global.Sessions.FirstOrDefault(s => s.PublicPids.Contains(friend.User.Pid) || s.PrivatePids.Contains(friend.User.Pid));
                        if (session == null)
                        {
                            Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: {name} is not in a valid session.", protocol, LogSource.RMC, Color.Red, client);
                            continue;
                        }

                        // Target the host of the session
                        ClientInfo host = Global.Clients.FirstOrDefault(c => c.User.Pid == session.HostPid);
                        if (host == null)
                        {
                            Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: host {session.HostPid} not found for session {session.Key.SessionId}", protocol, LogSource.RMC, Color.Red, client);
                            continue;
                        }

                        if (!session.IsJoinable())
                        {
                            Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: for {name} found session {session.Key.SessionId} but it's not joinable.", protocol, LogSource.RMC, Color.Red, client);
                            continue;
                        }

                        Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: {name}'s session found {friend.GameSessionID} — connecting to host PID {session.HostPid}", protocol, LogSource.RMC, Color.Orange, client);
                        client.GameSessionID = session.Key.SessionId;
                        client.InGameSession = true;
                        // Create session result with host URLs
                        GameSessionSearchResult searchResult = new GameSessionSearchResult(session, host);
                        gameSessions.Add(new GameSessionByPlayerInfo(friend, searchResult));
                    }
                    reply = new RMCPacketResponseContactsService_RetrieveGameSessionFromContact(gameSessions);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }
    }
}
