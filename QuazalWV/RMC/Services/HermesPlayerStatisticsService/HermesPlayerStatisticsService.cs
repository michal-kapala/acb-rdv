using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class HermesPlayerStatisticsService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.HermesPlayerStats;

        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 2:
                    rmc.request = new RMCPacketRequestHermesPlayerStatisticsService_SendPlayerStats(s);
                    break;
                case 3:
                    rmc.request = new RMCPacketRequestHermesPlayerStatisticsService_ReadPlayerStats(s);
                    break;
                case 4:
                    rmc.request = new RMCPacketRequestHermesPlayerStatisticsService_ReadStatsLeaderboardByRange(s);
                    break;
                case 5:
                    rmc.request = new RMCPacketRequestHermesPlayerStatisticsService_ReadStatsLeaderboardByRangeForPlayer(s);
                    break;
                case 6:
                    rmc.request = new RMCPacketRequestHermesPlayerStatisticsService_ReadStatsLeaderboardByPlayerNames(s);
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
                case 2:
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 3:
                    var reqReadPlayerStats = (RMCPacketRequestHermesPlayerStatisticsService_ReadPlayerStats)rmc.request;
                    reply = new RMCPacketResponseHermesPlayerStatisticsService_ReadPlayerStats(client, reqReadPlayerStats.Queries);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 4:
                    reply = new RMCPacketResponseHermesPlayerStatisticsService_ReadStatsLeaderboardByRange();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 5:
                    reply = new RMCPacketResponseHermesPlayerStatisticsService_ReadStatsLeaderboardByRangeForPlayer();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 6:
                    reply = new RMCPacketResponseHermesPlayerStatisticsService_ReadStatsLeaderboardByPlayerNames();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }
    }
}
