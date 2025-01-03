using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestFriendsService_BlackList : RMCPRequest
	{
		public uint Pid { get; set; }
		public uint Details { get; set; }

		public RMCPacketRequestFriendsService_BlackList(Stream s)
		{
			Pid = Helper.ReadU32(s);
			Details = Helper.ReadU32(s);
		}

		public override string ToString()
		{
			return "[BlackList Request]";
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteU32(m, Pid);
			Helper.WriteU32(m, Details);
			return m.ToArray();
		}
	}
}
