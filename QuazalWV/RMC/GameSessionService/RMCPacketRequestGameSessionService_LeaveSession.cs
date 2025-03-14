using System.IO;

namespace QuazalWV
{
    internal class RMCPacketRequestGameSessionService_LeaveSession : RMCPRequest
    {
        public GameSessionKey key;

        public RMCPacketRequestGameSessionService_LeaveSession(Stream s)
        {
            key = new GameSessionKey(s);
        }

        public override string PayloadToString()
        {
            return key.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();

            return m.ToArray();
        }

        public override string ToString()
        {
            return "[AddParticipants Request]";
        }
    }
}