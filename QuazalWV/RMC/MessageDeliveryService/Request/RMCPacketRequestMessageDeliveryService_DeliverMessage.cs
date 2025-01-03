using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestMessageDeliveryService_DeliverMessage : RMCPRequest
	{
		public TextMessage Message { get; set; }

		public RMCPacketRequestMessageDeliveryService_DeliverMessage(Stream s)
		{
			Message = new TextMessage(s);
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Message.ToBuffer(m);
			return m.ToArray();
		}

		public override string ToString()
		{
			return "[DeliverMessage Request]";
		}
	}
}
