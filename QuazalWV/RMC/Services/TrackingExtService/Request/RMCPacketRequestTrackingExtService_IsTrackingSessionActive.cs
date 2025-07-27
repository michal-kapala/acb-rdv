using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestTrackingExtService_IsTrackingSessionActive : RMCPRequest
    {
        public string TrackingSessionId { get; set; }

        public RMCPacketRequestTrackingExtService_IsTrackingSessionActive(Stream s)
        {
            TrackingSessionId = Helper.ReadString(s);
        }

        public override string ToString()
        {
            return "[IsTrackingSessionActive Request]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Session: {TrackingSessionId}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteString(m, TrackingSessionId);
            return m.ToArray();
        }
    }
}
