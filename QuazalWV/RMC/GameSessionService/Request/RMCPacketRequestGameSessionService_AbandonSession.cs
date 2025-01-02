using System;
using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestGameSessionService_AbandonSession : RMCPRequest
	{
		public GameSessionKey Key { get; set; }

		public RMCPacketRequestGameSessionService_AbandonSession(Stream s)
		{
			Key = new GameSessionKey(s);
		}

		public override string ToString()
		{
			return "[AbandonSession Request]";
		}

		public override string PayloadToString()
		{
			throw new NotImplementedException();
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Key.ToBuffer(m);
			return m.ToArray();
		}
	}
}
