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
                    Log.WriteLine(1, $"[RMC MessageDelivery] '{reqDeliver.Message.Body}' from {client.User.Pid} to {reqDeliver.Message.RecipientId}", Color.Blue, client);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    if (reqDeliver.Message.Subject != "Notification")
                    {
                        DBHelper.AddMessage(reqDeliver.Message);
                        var recipient = Global.Clients.Find(c => c.User.Pid == reqDeliver.Message.RecipientId);
                        if (recipient != null)
                        {
                            Log.WriteLine(1, $"[RMC MessageDelivery] Relaying message to {recipient.User.Name}", Color.Blue, client);
                            //RMC.SendRequest(recipient, reqDeliver, RMCP.PROTOCOL.MessageDeliveryService, 1);
                            NotificationManager.MessageReceived(recipient, client.User.Pid, reqDeliver.Message.Body);
                        }
                    }
                    break;
                default:
                    Log.WriteLine(1, $"[RMC MessageDelivery] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
