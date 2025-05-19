using System.IO;

namespace QuazalWV
{
    public class RMCPacketResponseGameSessionService_GetSession : RMCPResponse
    {
        public GameSessionSearchResult SearchResult { get; set; }

        public RMCPacketResponseGameSessionService_GetSession(Session ses)
        {
            SearchResult = new GameSessionSearchResult(ses);
        }

        public override string ToString()
        {
            return "[GetSession Response]";
        }

        public override string PayloadToString()
        {
            return SearchResult.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            SearchResult.ToBuffer(m);
            return m.ToArray();
        }
    }
}
