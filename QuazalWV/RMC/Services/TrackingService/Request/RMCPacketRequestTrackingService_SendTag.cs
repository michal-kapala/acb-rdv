using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestTrackingService_SendTag : RMCPRequest
    {
        public uint TrackingId { get; set; }
        public string Tag { get; set; }
        public string Attributes { get; set; }
        public uint DeltaTime { get; set; }

        public RMCPacketRequestTrackingService_SendTag(Stream s)
        {
            TrackingId = Helper.ReadU32(s);
            Tag = Helper.ReadString(s);
            Attributes = Helper.ReadString(s);
            DeltaTime = Helper.ReadU32(s);
        }

        public override byte[] ToBuffer()
        {
            MemoryStream result = new MemoryStream();
            Helper.WriteU32(result, TrackingId);
            Helper.WriteString(result, Tag);
            Helper.WriteString(result, Attributes);
            Helper.WriteU32(result, DeltaTime);
            return result.ToArray();
        }

        public override string ToString()
        {
            return "[SendTag Request]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[TrackingId: {TrackingId}]");
            sb.AppendLine($"\t[Tag: {Tag}]");
            sb.AppendLine($"\t[Attributes: {Attributes}]");
            sb.AppendLine($"\t[DeltaTime: {DeltaTime}]");
            return sb.ToString();
        }
    }
}
