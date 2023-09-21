using System.IO;

namespace QuazalWV
{
    public class ExternalAccount : IData
    {
        public uint BasicStatus { get; set; }
        public bool MissingRequiredInformation { get; set; }
        public bool RecoveringPassword { get; set; }
        public bool PendingDeactivation { get; set; }

        public ExternalAccount(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            BasicStatus = Helper.ReadU32(s);
            MissingRequiredInformation = Helper.ReadBool(s);
            RecoveringPassword = Helper.ReadBool(s);
            PendingDeactivation = Helper.ReadBool(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, BasicStatus);
            Helper.WriteBool(s, MissingRequiredInformation);
            Helper.WriteBool(s, RecoveringPassword);
            Helper.WriteBool(s, PendingDeactivation);
        }
    }
}
