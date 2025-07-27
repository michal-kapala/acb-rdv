using System.IO;

namespace QuazalWV
{
    public class RMCPacketRequestNATTraversalService_InitiateProbe : RMCPRequest
    {
        public StationUrl Url {  get; set; }

        public RMCPacketRequestNATTraversalService_InitiateProbe(StationUrl url)
        {
            Url = url;
        }

        public override string PayloadToString()
        {
            return $"[{Url}]";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteString(m, Url.ToString());
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[InitiateProbe Request]";
        }
    }
}
