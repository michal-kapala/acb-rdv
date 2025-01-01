using System.IO;

namespace QuazalWV
{
	public class PresenceElement : IData
	{
		public uint UnkInt1 {  get; set; }
		public bool UnkBool {  get; set; }
		public uint UnkInt2 { get; set; }
		public QBuffer Buffer { get; set; }

		public PresenceElement()
		{
			Buffer = new QBuffer();
		}

		public PresenceElement(Stream s)
		{
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			UnkInt1 = Helper.ReadU32(s);
			UnkBool = Helper.ReadBool(s);
			UnkInt2 = Helper.ReadU32(s);
			Buffer = new QBuffer(s);
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, UnkInt1);
			Helper.WriteBool(s, UnkBool);
			Helper.WriteU32(s, UnkInt2);
			Buffer.ToBuffer(s);
		}
	}
}
