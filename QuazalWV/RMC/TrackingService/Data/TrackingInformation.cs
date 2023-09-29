using System.IO;

namespace QuazalWV
{
    public class TrackingInformation : IData
    {
        public uint Ipn { get; set; }
        public string UserId { get; set; }
        public string MachineId { get; set; }
        public string VisitorId { get; set; }
        public string UtsVersion { get; set; }

        public TrackingInformation()
        {
            
        }

        public TrackingInformation(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Ipn = Helper.ReadU32(s);
            UserId = Helper.ReadString(s);
            MachineId = Helper.ReadString(s);
            VisitorId = Helper.ReadString(s);
            UtsVersion = Helper.ReadString(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, Ipn);
            Helper.WriteString(s, UserId);
            Helper.WriteString(s, MachineId);
            Helper.WriteString(s, VisitorId);
            Helper.WriteString(s, UtsVersion);
        }
    }
}
