using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class ContactsService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 3:
                    rmc.request = new RMCPacketRequestContactsService_RetrieveGameSessionFromContact(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Contacts] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
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
                        Log.WriteLine(1, $"[RMC Contacts] Requesting {name}'s game session", Color.Green, client);
                        friend = Global.Clients.Find(c => c.User != null && c.User.Name == name);
                        if (friend == null)
                        {
                            Log.WriteLine(1, $"[RMC Contacts] RetrieveGameSessionFromContact for {name} who's not currently online.", Color.Red, client);
                            continue;
                        }
                        else if (!friend.InGameSession)
                        {
                            Log.WriteLine(1, $"[RMC Contacts] RetrieveGameSessionFromContact for {name} who's not currently in a session.", Color.Red, client);
                            continue;
                        }
                        session = Global.Sessions.Find(s => s.Key.SessionId == friend.GameSessionID);
                        if (session == null)
                        {
                            Log.WriteLine(1, $"[RMC Contacts] RetrieveGameSessionFromContact for {name} didn't find session {friend.GameSessionID}.", Color.Red, client);
                            continue;
                        }
                        gameSessions.Add(new GameSessionByPlayerInfo(friend, new GameSessionSearchResult(session, friend)));
                    }
                    reply = new RMCPacketResponseContactsService_RetrieveGameSessionFromContact(gameSessions);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Contacts] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
