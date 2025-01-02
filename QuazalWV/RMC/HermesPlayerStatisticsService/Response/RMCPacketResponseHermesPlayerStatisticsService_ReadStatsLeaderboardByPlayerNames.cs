using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class RMCPacketResponseHermesPlayerStatisticsService_ReadStatsLeaderboardByPlayerNames : RMCPResponse
	{
		public List<PlayerStatContainer> Containers { get; set; }
		public uint UnkInt { get; set; }

		public RMCPacketResponseHermesPlayerStatisticsService_ReadStatsLeaderboardByPlayerNames()
		{
			Containers = new List<PlayerStatContainer>();
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteU32(m, (uint)Containers.Count);
			foreach (PlayerStatContainer container in Containers)
				container.ToBuffer(m);
			Helper.WriteU32(m, UnkInt);
			return m.ToArray();
		}

		public override string ToString()
		{
			return "[ReadStatsLeaderboardByPlayerNames Response]";
		}
	}
}
