using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class FriendsService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 4:
                    rmc.request = new RMCPacketRequestFriendsService_AddFriendByNameWithDetails(s);
                    break;
                case 9:
                    rmc.request = new RMCPacketRequestFriendsService_ClearRelationship(s);
                    break;
                case 12:
                    rmc.request = new RMCPacketRequestFriendsService_GetDetailedList(s);
                    break;
                case 13:
                    rmc.request = new RMCPacketRequestFriendsService_GetRelationships(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Friends] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 4:
                    var reqAddFriendByName = (RMCPacketRequestFriendsService_AddFriendByNameWithDetails)rmc.request;
                    reply = new RMCPacketResponseFriendsService_AddFriendByNameWithDetails(4662, reqAddFriendByName.Invitee);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 9:
                    reply = new RMCPacketResponseFriendsService_ClearRelationship();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 12:
                    var reqGetDetailedList = (RMCPacketRequestFriendsService_GetDetailedList)rmc.request;
                    reply = new RMCPacketResponseFriendsService_GetDetailedList(reqGetDetailedList.Relationship);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 13:
                    reply = new RMCPacketResponseFriendsService_GetRelationships();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Friends] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
