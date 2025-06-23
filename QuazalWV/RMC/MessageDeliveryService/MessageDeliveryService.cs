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
                    Log.WriteLine(1, $"[RMC MessageDelivery] '{reqDeliver.Message.Body}' from {reqDeliver.Message.SenderName} to {reqDeliver.Message.RecipientId}", Color.Blue, client);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC MessageDelivery] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
