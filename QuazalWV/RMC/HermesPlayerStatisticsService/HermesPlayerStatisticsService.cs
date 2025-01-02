using System.Drawing;
using System.IO;

namespace QuazalWV
{
	public static class HermesPlayerStatisticsService
	{
		public static void ProcessRequest(Stream s, RMCP rmc)
		{
			switch (rmc.methodID)
			{
				case 3:
					rmc.request = new RMCPacketRequestHermesPlayerStatisticsService_ReadPlayerStats(s);
					break;
				case 4:
					rmc.request = new RMCPacketRequestHermesPlayerStatisticsService_ReadStatsLeaderboardByRange(s);
					break;
				case 5:
					rmc.request = new RMCPacketRequestHermesPlayerStatisticsService_ReadStatsLeaderboardByRangeForPlayer(s);
					break;
				default:
					Log.WriteLine(1, $"[RMC HermesPlayerStats] Error: Unknown Method {rmc.methodID}", Color.Red);
					break;
			}
		}

		public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
		{
			RMCPResponse reply;
			switch (rmc.methodID)
			{
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
				default:
					Log.WriteLine(1, $"[RMC HermesPlayerStats] Error: Unknown Method {rmc.methodID}", Color.Red, client);
					break;
			}
		}
	}
}
