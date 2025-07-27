using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestGameSessionService_GetInvitationsReceived : RMCPRequest
    {
        public uint GameSessionTypeId { get; set; }
        public ResultRange ResultRange { get; set; }

        public RMCPacketRequestGameSessionService_GetInvitationsReceived(Stream s)
        {
            GameSessionTypeId = Helper.ReadU32(s);
            ResultRange = new ResultRange(s);
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Session type: {GameSessionTypeId}]");
            sb.AppendLine($"\t[Max count: {ResultRange.Size}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, GameSessionTypeId);
            ResultRange.ToBuffer(m);
            return m.ToArray();
        }

        public override string ToString()
        {
            return $"[GetInvitationsReceived Request]";
        }
    }
}
