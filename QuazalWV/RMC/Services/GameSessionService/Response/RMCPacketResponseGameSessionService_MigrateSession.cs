using System.IO;

namespace QuazalWV
{
    public class RMCPacketResponseGameSessionService_MigrateSession : RMCPResponse
    {
        public GameSessionKey MigratedKey { get; set; }

        public RMCPacketResponseGameSessionService_MigrateSession(GameSessionKey newKey)
        {
            MigratedKey = newKey;
        }

        public override string PayloadToString()
        {
            return MigratedKey.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            MigratedKey.ToBuffer(m);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[MigrateSession Response]";
        }
    }
}
