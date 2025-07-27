using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestTrackingService_SendUserInfo : RMCPRequest
    {
        public TrackingInformation TrackingInfo { get; set; }
        public uint DeltaTime { get; set; }

        public RMCPacketRequestTrackingService_SendUserInfo(Stream s)
        {
            TrackingInfo = new TrackingInformation(s);
            DeltaTime = Helper.ReadU32(s);
        }

        public override string ToString()
        {
            return "[SendUserInfo Request]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Machine: {TrackingInfo.MachineId}]");
            sb.AppendLine($"\t[Visitor: {TrackingInfo.VisitorId}]");
            sb.AppendLine($"\t[Tracking version: {TrackingInfo.UtsVersion}]");
            sb.AppendLine($"\t[Time: {DeltaTime}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            TrackingInfo.ToBuffer(m);
            Helper.WriteU32(m, DeltaTime);
            return m.ToArray();
        }
    }
}
