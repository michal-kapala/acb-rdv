using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class GameInfoService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.GameInfo;

        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 5:
                    rmc.request = new RMCPacketRequestGameInfoService_GetFileListOnGameStorage(s);
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
                case 5:
                    var rGetFiles = (RMCPacketRequestGameInfoService_GetFileListOnGameStorage)rmc.request;
                    reply = new RMCPacketResponseGameInfoService_GetFileListOnGameStorage(rGetFiles.FileName);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }
    }
}
