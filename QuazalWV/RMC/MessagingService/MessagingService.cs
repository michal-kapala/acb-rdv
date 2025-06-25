using System;
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

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 3:
                    var reqGetHeaders = (RMCPacketRequestMessagingService_GetMessagesHeaders)rmc.request;
                    Log.WriteLine(1, $"[RMC Messaging] Recipient.Pid: {reqGetHeaders.Recipient.Pid}", Color.Blue, client);
                    var msgs = DBHelper.GetMessagesByRecipient(reqGetHeaders.Recipient);
                    foreach (var msg in msgs)
                        msg.ReceptionTime = new QDateTime(DateTime.Now);
                    var headers = new List<UserMessage>();
                    foreach (var msg in msgs)
                        headers.Add(msg.ToHeader());
                    Log.WriteLine(1, $"[RMC Messaging] Headers: {msgs.Count}", Color.Blue, client);
                    reply = new RMCPacketResponseMessagingService_GetMessagesHeaders(headers);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 5:
                    var reqRecvMsgs = (RMCPacketRequestMessagingService_RetrieveMessages)rmc.request;
                    var messages = DBHelper.GetMessagesByIds(reqRecvMsgs.MessageIds);
                    var timestamp = DateTime.Now;
                    foreach (var msg in messages)
                        msg.ReceptionTime = new QDateTime(timestamp);
                    reply = new RMCPacketResponseMessagingService_RetrieveMessages(messages);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    DBHelper.UpdateDeliveredMessages(reqRecvMsgs.MessageIds, timestamp);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Messaging] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
