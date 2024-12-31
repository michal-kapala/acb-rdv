using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestNewsService_GetNewsHeaders : RMCPRequest
	{
		public NewsRecipient Recipient { get; set; }
		public ResultRange Range { get; set; }

		public RMCPacketRequestNewsService_GetNewsHeaders(Stream s)
		{
			Recipient = new NewsRecipient(s);
			Range = new ResultRange(s);
		}

		public override string ToString()
		{
			return "[GetNewsHeaders Request]";
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Recipient.ToBuffer(m);
			Range.ToBuffer(m);
			return m.ToArray();
		}
	}
}
