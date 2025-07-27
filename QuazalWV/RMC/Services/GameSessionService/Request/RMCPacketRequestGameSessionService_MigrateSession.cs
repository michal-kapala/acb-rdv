using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestGameSessionService_MigrateSession : RMCPRequest
	{
		public GameSessionKey Key { get; set; }

		public RMCPacketRequestGameSessionService_MigrateSession(Stream s)
		{
			Key = new GameSessionKey(s);
		}

		public override string ToString()
		{
			return "[MigrateSession Request]";
		}

		public override string PayloadToString()
		{
			return Key.ToString();
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Key.ToBuffer(m);
			return m.ToArray();
		}
	}
}
