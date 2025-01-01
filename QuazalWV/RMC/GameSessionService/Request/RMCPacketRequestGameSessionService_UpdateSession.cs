using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestGameSessionService_UpdateSession : RMCPRequest
	{
		public GameSessionUpdate SessionUpdate { get; set; }

		public RMCPacketRequestGameSessionService_UpdateSession(Stream s)
		{
			SessionUpdate = new GameSessionUpdate(s);
		}

		public override string ToString()
		{
			return "[UpdateSession Request]";
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			SessionUpdate.ToBuffer(m);
			return m.ToArray();
		}
	}
}
