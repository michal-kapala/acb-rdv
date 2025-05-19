using System.IO;

namespace QuazalWV
{
    public class RMCPacketRequestGameSessionService_GetSession : RMCPRequest
    {
        public GameSessionKey Key { get; set; }

        public RMCPacketRequestGameSessionService_GetSession(Stream s)
        {
            Key = new GameSessionKey(s);
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

        public override string ToString()
        {
            return $"[GetSession Request]";
        }
    }
}
