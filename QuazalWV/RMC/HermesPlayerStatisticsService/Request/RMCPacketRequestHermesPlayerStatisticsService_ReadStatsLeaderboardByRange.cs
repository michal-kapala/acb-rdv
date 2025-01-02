using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestHermesPlayerStatisticsService_ReadStatsLeaderboardByRange : RMCPRequest
	{
		public uint UnkInt1 { get; set; }
		public uint UnkInt2 { get; set; }
		public uint UnkInt3 { get; set; }
		public uint UnkInt4 { get; set; }
		public uint UnkInt5 { get; set; }
		public uint UnkInt6 { get; set; }

		public RMCPacketRequestHermesPlayerStatisticsService_ReadStatsLeaderboardByRange(Stream s)
		{
			UnkInt1 = Helper.ReadU32(s);
			UnkInt2 = Helper.ReadU32(s);
			UnkInt3 = Helper.ReadU32(s);
			UnkInt4 = Helper.ReadU32(s);
			UnkInt5 = Helper.ReadU32(s);
			UnkInt6 = Helper.ReadU32(s);
		}

		public override string ToString()
		{
			return "[ReadStatsLeaderboardByRange Request]";
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteU32(m, UnkInt1);
			Helper.WriteU32(m, UnkInt2);
			Helper.WriteU32(m, UnkInt3);
			Helper.WriteU32(m, UnkInt4);
			Helper.WriteU32(m, UnkInt5);
			Helper.WriteU32(m, UnkInt6);
			return m.ToArray();
		}
	}
}
