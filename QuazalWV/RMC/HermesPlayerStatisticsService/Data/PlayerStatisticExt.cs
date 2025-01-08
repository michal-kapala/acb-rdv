using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class PlayerStatisticExt : IData
	{
		public uint Id { get; set; }
		public uint UnkInt1 { get; set; }
		public uint UnkInt2 { get; set; }
		public List<PlayerStatisticInfoExt> Infos { get; set; }
		public QDateTime DateTime { get; set; }

		public PlayerStatisticExt(StatQuery query)
		{
			Id = query.StatId;
			UnkInt1 = 0;
			UnkInt2 = 0;
			Infos = new List<PlayerStatisticInfoExt>();
			foreach (uint id in query.InfoIds)
				Infos.Add(new PlayerStatisticInfoExt((byte)id, (byte)Id));
			DateTime = new QDateTime(System.DateTime.Now);
		}

		public PlayerStatisticExt(Stream s)
		{
			Infos = new List<PlayerStatisticInfoExt>();
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			Id = Helper.ReadU32(s);
			UnkInt1 = Helper.ReadU32(s);
			UnkInt2 = Helper.ReadU32(s);
			uint count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				Infos.Add(new PlayerStatisticInfoExt(s));
			DateTime = new QDateTime(s);
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, Id);
			Helper.WriteU32(s, UnkInt1);
			Helper.WriteU32(s, UnkInt2);
			Helper.WriteU32(s, (uint)Infos.Count);
			foreach (PlayerStatisticInfoExt info in Infos)
				info.ToBuffer(s);
			DateTime.ToBuffer(s);
		}
	}
}
