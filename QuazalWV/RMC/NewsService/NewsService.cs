using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class NewsService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 2:
                    rmc.request = new RMCPacketRequestNewsService_GetChannelsByTypes(s);
                    break;
                case 8:
                    rmc.request = new RMCPacketRequestNewsService_GetNewsHeaders(s);
                    break;
                case 10:
                    rmc.request = new RMCPacketRequestNewsService_GetNumberOfNews(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC News] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 2:
                    reply = new RMCPacketResponseNewsService_GetChannelsByTypes();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 8:
                    reply = new RMCPacketResponseNewsService_GetNewsHeaders();
					RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
					break;
                case 10:
                    var getNewsNumber = (RMCPacketRequestNewsService_GetNumberOfNews)rmc.request;
                    reply = new RMCPacketResponseNewsService_GetNumberOfNews();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC News] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
