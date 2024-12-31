using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class PlayerStatContainer : IData
	{
		public uint UnkUint { get; set; }
		public string PlayerName { get; set; }
		public List<PlayerStatisticExt> Stats { get; set; }

		public PlayerStatContainer(string player, List<StatQuery> queries)
		{
			UnkUint = 0;
			PlayerName = player;
			Stats = new List<PlayerStatisticExt>();
			foreach (var query in queries)
				Stats.Add(new PlayerStatisticExt(query));
		}

		public PlayerStatContainer(Stream s)
		{
			Stats = new List<PlayerStatisticExt>();
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			UnkUint = Helper.ReadU32(s);
			PlayerName = Helper.ReadString(s);
			uint count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				Stats.Add(new PlayerStatisticExt(s));
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, UnkUint);
			Helper.WriteString(s, PlayerName);
			Helper.WriteU32(s, (uint)Stats.Count);
			foreach (PlayerStatisticExt stat in Stats)
				stat.ToBuffer(s);
		}
	}
}
