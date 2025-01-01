using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestGameSessionService_CreateSession : RMCPRequest
	{
		public GameSession Session { get; set; }

		public RMCPacketRequestGameSessionService_CreateSession(Stream s)
		{
			Session = new GameSession(s);
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Session.ToBuffer(m);
			return m.ToArray();
		}

		public override string ToString()
		{
			return "[CreateSession Request]";
		}
	}
}
