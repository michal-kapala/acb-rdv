using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class RMCPacketResponseNewsService_GetNewsHeaders : RMCPResponse
	{
		public List<NewsHeader> Headers { get; set; }

		public RMCPacketResponseNewsService_GetNewsHeaders()
		{
			Headers = new List<NewsHeader>();
		}

		public override string ToString()
		{
			return "[GetNewsHeaders Response]";
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteU32(m, (uint)Headers.Count);
			foreach (NewsHeader header in Headers)
				header.ToBuffer(m);
			return m.ToArray();
		}
	}
}
