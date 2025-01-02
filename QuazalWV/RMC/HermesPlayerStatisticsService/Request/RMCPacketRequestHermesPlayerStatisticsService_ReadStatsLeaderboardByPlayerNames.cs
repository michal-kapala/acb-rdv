using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestHermesPlayerStatisticsService_ReadStatsLeaderboardByPlayerNames : RMCPRequest
	{
		public List<PlayerQuery> Queries { get; set; }
		public List<string> Players { get; set; }

		public RMCPacketRequestHermesPlayerStatisticsService_ReadStatsLeaderboardByPlayerNames(Stream s)
		{
			Queries = new List<PlayerQuery>();
			Players = new List<string>();

			uint count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				Queries.Add(new PlayerQuery(s));
			count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				Players.Add(Helper.ReadString(s));
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteU32(m, (uint)Queries.Count);
			foreach (PlayerQuery query in Queries)
				query.ToBuffer(m);
			Helper.WriteU32(m, (uint)Players.Count);
			foreach (string player in Players)
				Helper.WriteString(m, player);
			return m.ToArray();
		}

		public override string ToString()
		{
			return "[ReadStatsLeaderboardByPlayerNames Request]";
		}
	}
}
