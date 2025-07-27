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

		public PlayerStatisticValues(uint score, uint long1, uint long2, string unkStr, byte unkBytes)
		{
			UnkInt = score;
			UnkLong1 = long1;
			UnkLong2 = long2;
			UnkStr = unkStr;
			UnkByte = unkBytes;
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
