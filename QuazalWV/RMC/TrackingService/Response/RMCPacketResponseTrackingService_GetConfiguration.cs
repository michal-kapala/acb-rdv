using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseTrackingService_GetConfiguration : RMCPResponse
    {
        public List<string> Tags { get; set; }

        public RMCPacketResponseTrackingService_GetConfiguration(List<string> tags)
        {
            Tags = tags;
        }

        public override string ToString()
        {
            return "[GetConfiguration Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string tag in Tags)
                sb.AppendLine($"\t[{tag}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Tags.Count);
            foreach (string tag in Tags)
                Helper.WriteString(m, tag);
            return m.ToArray();
        }
    }
}
