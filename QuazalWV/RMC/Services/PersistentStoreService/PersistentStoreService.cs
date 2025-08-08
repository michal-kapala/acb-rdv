using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class PersistentStoreService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.PersistentStore;

        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 4:
                    rmc.request = new RMCPacketRequestPersistentStoreService_GetItem(s);
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
                case 4:
                    const string fileName = "gamesettings_c1380_d873_s6285.cxb";
                    reply = new RMCPacketResponsePersistentStoreService_GetItem(fileName);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }
    }
}
