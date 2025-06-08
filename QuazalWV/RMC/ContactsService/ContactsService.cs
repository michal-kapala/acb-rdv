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
                    Log.WriteLine(1, $"[SessionId: {reqGetFriendSessions.SessionType}]", Color.Green, client);
                    foreach (string s in reqGetFriendSessions.FriendNames)
                        Log.WriteLine(1, $"[{s}]", Color.Green, client);
                    reply = new RMCPacketResponseContactsService_RetrieveGameSessionFromContact();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Contacts] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
