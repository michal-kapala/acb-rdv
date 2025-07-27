using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseTrackingService_SendUserInfo : RMCPResponse
    {
        public TrackingInformation TrackingInfo { get; set; }
        public uint TrackingId { get; set; }

        public RMCPacketResponseTrackingService_SendUserInfo(ClientInfo client)
        {
            // dumped from the original traffic
            TrackingInfo = new TrackingInformation()
            {
                Ipn = client.TrackingUser.Pid,
                UserId = "",
                MachineId = "e8de277b6f25",
                VisitorId = "95E866FC-1685-41E3-9DE9-D78227FD61F9",
                UtsVersion = "30053"
            };
            TrackingId = 0x5A7A0090;
        }

        public override string ToString()
        {
            return "[SendUserInfo Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Machine: {TrackingInfo.MachineId}]");
            sb.AppendLine($"\t[Visitor: {TrackingInfo.VisitorId}]");
            sb.AppendLine($"\t[Tracking version: {TrackingInfo.UtsVersion}]");
            sb.AppendLine($"\t[Tracking ID: {TrackingId}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            TrackingInfo.ToBuffer(m);
            Helper.WriteU32(m, TrackingId);
            return m.ToArray();
        }
    }
}
