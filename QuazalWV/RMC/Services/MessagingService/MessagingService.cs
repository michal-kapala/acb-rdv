using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public class MessagingService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 3:
                    rmc.request = new RMCPacketRequestMessagingService_GetMessagesHeaders(s);
                    break;
                case 5:
                    rmc.request = new RMCPacketRequestMessagingService_RetrieveMessages(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Messaging] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(PrudpPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 3:
                    var reqGetHeaders = (RMCPacketRequestMessagingService_GetMessagesHeaders)rmc.request;
                    var msgs = DbHelper.GetPendingMessages(reqGetHeaders.Recipient);
                    var headers = new List<UserMessage>();
                    foreach (var msg in msgs)
                        headers.Add(msg.ToHeader());
                    reply = new RMCPacketResponseMessagingService_GetMessagesHeaders(headers);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 5:
                    var reqRecvMsgs = (RMCPacketRequestMessagingService_RetrieveMessages)rmc.request;
                    var messages = DbHelper.GetMessagesByIds(reqRecvMsgs.MessageIds);
                    reply = new RMCPacketResponseMessagingService_RetrieveMessages(messages);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    DbHelper.UpdateDeliveredMessages(reqRecvMsgs.MessageIds);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Messaging] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
