using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
    public class RMCPacketResponseContactsService_RetrieveGameSessionFromContact : RMCPResponse
    {
        public List<GameSessionByPlayerInfo> GameSessions { get; set; }

        public RMCPacketResponseContactsService_RetrieveGameSessionFromContact()
        {
            GameSessions = new List<GameSessionByPlayerInfo>();
        }

        public override string PayloadToString()
        {
            return "";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)GameSessions.Count);
            foreach (var result in GameSessions)
                result.ToBuffer(m);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[RetrieveGameSessionFromContact Response]";
        }
    }
}
