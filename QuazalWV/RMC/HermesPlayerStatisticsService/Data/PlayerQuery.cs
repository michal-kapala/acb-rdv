using System.IO;

namespace QuazalWV
{
	public class PlayerQuery : IData
	{
		public uint UnkInt1 { get; set; }
		public uint UnkInt2 { get; set; }

		public PlayerQuery()
		{
			
		}

		public PlayerQuery(Stream s)
		{
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			UnkInt1 = Helper.ReadU32(s);
			UnkInt2 = Helper.ReadU32(s);
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, UnkInt1);
			Helper.WriteU32(s, UnkInt2);
		}
	}
}
