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
                        Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: requesting {name}'s game session", protocol, LogSource.RMC, Color.Green, client);
                        ClientInfo friend = Global.Clients.Find(c => c.User != null && c.User.Name == name);
                        if (friend == null)
                        {
                            Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: for {name} who's not currently online.", protocol, LogSource.RMC, Color.Red, client);
                            continue;
                        }
                        Session session = Global.Sessions.Find(s => s.Key.SessionId == friend.GameSessionID);
                        // Fix: Attempt to detect and correct desync
                        if (session == null)
                        {
                            // Check whether the session ID is incorrect or outdated
                            Session possibleSession = Global.Sessions.FirstOrDefault(s =>
                                s.PublicPids.Contains(friend.User.Pid) || s.PrivatePids.Contains(friend.User.Pid));

                            if (possibleSession != null)
                            {
                                Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: fixed {name}'s session: was {friend.GameSessionID}, now {possibleSession.Key.SessionId}", protocol, LogSource.RMC, Color.Orange, client);
                                friend.GameSessionID = possibleSession.Key.SessionId;
                                session = possibleSession;
                                friend.InGameSession = true;
                            }
                            else
                            {
                                if (!friend.InGameSession)
                                    Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: {name} not currently in a session.", protocol, LogSource.RMC, Color.Red, client);
                                else
                                    Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: {name}'s session {friend.GameSessionID} not found (may have been deleted).", protocol, LogSource.RMC, Color.Red, client);
                                continue;
                            }
                        }
                        if (!session.IsJoinable())
                        {
                            Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: for {name} found session {session.Key.SessionId} but it's not joinable.", protocol, LogSource.RMC, Color.Red, client);
                            continue;
                        }
                        Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: joining session {friend.GameSessionID}", protocol, LogSource.RMC, Color.Orange, client);
                        client.GameSessionID = session.Key.SessionId;
                        client.InGameSession = true;
                        // Determine the host for the session URLs
                        ClientInfo host = Global.Clients.Find(c => c.User.Pid == session.HostPid);
                        if (host == null)
                        {
                            Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact: host {session.HostPid} not found for session {session.Key.SessionId}", protocol, LogSource.RMC, Color.Red, client);
                            continue;
                        }
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
