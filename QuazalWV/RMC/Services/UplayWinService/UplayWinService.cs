using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class UplayWinService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.UplayWin;

        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 5:
                    rmc.request = new RMCPacketRequestUplayWinService_GetRewards(s);
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
                    var getRewards = (RMCPacketRequestUplayWinService_GetRewards)rmc.request;
                    var rewards = DbHelper.GetRewards("PC");
                    reply = new RMCPacketResponseUplayWinService_GetRewards(rewards);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }
    }
}
