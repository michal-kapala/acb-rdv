using System.IO;

namespace QuazalWV
{
	public class RMCPacketResponseUbiNewsService_GetNewsChannel : RMCPResponse
	{
		public UbiNewsChannel Channel { get; set; }

		public RMCPacketResponseUbiNewsService_GetNewsChannel(uint pid)
		{
			Channel = new UbiNewsChannel(pid);
		}

		public override string ToString()
		{
			return "[GetNewsChannel Response]";
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Channel.ToBuffer(m);
			return m.ToArray();
		}
	}
}
