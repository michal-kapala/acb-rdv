using System.IO;

namespace QuazalWV
{
	internal class RMCPacketResponseFriendsService_BlackList : RMCPResponse
	{
		public bool RetVal { get; set; }

		public RMCPacketResponseFriendsService_BlackList()
		{
			RetVal = true;
		}

		public override string ToString()
		{
			return "[BlackList Response]";
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteBool(m, RetVal);
			return m.ToArray();
		}
	}
}
