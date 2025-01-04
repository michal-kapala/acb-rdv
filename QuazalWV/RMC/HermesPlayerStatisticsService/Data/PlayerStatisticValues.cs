using System.IO;

namespace QuazalWV
{
	public class PlayerStatisticValues : IData
	{
		public uint UnkInt { get; set; }
		public ulong UnkLong1 { get; set; }
		public ulong UnkLong2 { get; set; }
		public string UnkStr { get; set; }
		public byte UnkByte { get; set; }

		public PlayerStatisticValues()
		{
			UnkInt = 650000; // seems to be best session score but it influences xp so 650 000 forces level 50 (max level)
			UnkLong1 = 0;
			UnkLong2 = 0;
			UnkStr = "";
			UnkByte = 1;
		}

		public PlayerStatisticValues(Stream s)
		{
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			UnkInt = Helper.ReadU32(s);
			UnkLong1 = Helper.ReadU64(s);
			UnkLong2 = Helper.ReadU64(s);
			UnkStr = Helper.ReadString(s);
			UnkByte = Helper.ReadU8(s);
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, UnkInt);
			Helper.WriteU64(s, UnkLong1);
			Helper.WriteU64(s, UnkLong2);
			Helper.WriteString(s, UnkStr);
			Helper.WriteU8(s, UnkByte);
		}
	}
}
