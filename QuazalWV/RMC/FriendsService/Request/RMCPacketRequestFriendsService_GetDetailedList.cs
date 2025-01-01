using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestFriendsService_GetDetailedList : RMCPRequest
	{
		public byte Relationship {  get; set; }
		public bool Reversed { get; set; }

		public RMCPacketRequestFriendsService_GetDetailedList(Stream s)
		{
			Relationship = Helper.ReadU8(s);
			Reversed = Helper.ReadBool(s);
		}

		public override string ToString()
		{
			return "[GetDetailedList Request]";
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteU8(m, Relationship);
			Helper.WriteBool(m, Reversed);
			return m.ToArray();
		}
	}
}
