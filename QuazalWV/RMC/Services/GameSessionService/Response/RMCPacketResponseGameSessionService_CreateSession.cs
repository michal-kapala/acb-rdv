using System.IO;

namespace QuazalWV
{
	public class RMCPacketResponseGameSessionService_CreateSession : RMCPResponse
	{
		public GameSessionKey SessionKey { get; set; }

		public RMCPacketResponseGameSessionService_CreateSession(uint type, uint sesId)
		{
			SessionKey = new GameSessionKey
			{
				TypeId = type,
				SessionId = sesId
			};
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			SessionKey.ToBuffer(m);
			return m.ToArray();
		}

		public override string ToString()
		{
			return "[CreateSession Response]";
		}
	}
}
