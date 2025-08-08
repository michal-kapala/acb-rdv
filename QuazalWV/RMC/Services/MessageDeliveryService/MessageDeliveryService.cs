using System;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class MessageDeliveryService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.MessageDelivery;

        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestMessageDeliveryService_DeliverMessage(s);
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
                            RMC.SendRequest(recipient, reqDeliver, RMCP.PROTOCOL.MessageDelivery, 1);
                    }
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }
    }
}
