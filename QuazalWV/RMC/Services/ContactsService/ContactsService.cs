using System.Collections.Generic;
using System.Drawing;
using System.IO;

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
                    ClientInfo friend;
                    Session session;
                    List<GameSessionByPlayerInfo> gameSessions = new List<GameSessionByPlayerInfo>();
                    foreach (string name in reqGetFriendSessions.FriendNames)
                    {
                        Log.WriteRmcLine(1, $"Requesting {name}'s game session", protocol, LogSource.RMC, Color.Green, client);
                        friend = Global.Clients.Find(c => c.User != null && c.User.Name == name);
                        if (friend == null)
                        {
                            Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact for {name} who's not currently online.", protocol, LogSource.RMC, Color.Red, client);
                            continue;
                        }
                        else if (!friend.InGameSession)
                        {
                            Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact for {name} who's not currently in a session.", protocol, LogSource.RMC, Color.Red, client);
                            continue;
                        }
                        session = Global.Sessions.Find(s => s.Key.SessionId == friend.GameSessionID);
                        if (session == null)
                        {
                            Log.WriteRmcLine(1, $"RetrieveGameSessionFromContact for {name} didn't find session {friend.GameSessionID}.", protocol, LogSource.RMC, Color.Red, client);
                            continue;
                        }
                        gameSessions.Add(new GameSessionByPlayerInfo(friend, new GameSessionSearchResult(session, friend)));
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
