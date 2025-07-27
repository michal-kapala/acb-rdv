using System;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class MessageDeliveryService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestMessageDeliveryService_DeliverMessage(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC MessageDelivery] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    reply = new RMCPResponseEmpty();
                    var reqDeliver = (RMCPacketRequestMessageDeliveryService_DeliverMessage)rmc.request;
                    reqDeliver.Message.SenderId = client.User.Pid;
                    reqDeliver.Message.SenderName = client.User.Name;
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    if (reqDeliver.Message.Subject != "Notification")
                    {
                        reqDeliver.Message.ReceptionTime = new QDateTime(DateTime.Now);
                        DbHelper.AddMessage(reqDeliver.Message);
                    }
                    else if (reqDeliver.Message.Body == "Check for new Messages")
                    {
                        var recipient = Global.Clients.Find(c => c.User.Pid == reqDeliver.Message.RecipientId);
                        if (recipient != null)
                            RMC.SendRequest(recipient, reqDeliver, RMCP.PROTOCOL.MessageDeliveryService, 1);
                    }
                    break;
                default:
                    Log.WriteLine(1, $"[RMC MessageDelivery] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
