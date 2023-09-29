using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestPersistentStoreService_GetItem : RMCPRequest
    {
        public uint Group { get; set; }
        public string Tag { get; set; }

        public RMCPacketRequestPersistentStoreService_GetItem(Stream s)
        {
            Group = Helper.ReadU32(s);
            Tag = Helper.ReadString(s);
        }

        public override string ToString()
        {
            return "[GetItem Request]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Group: {Group}]");
            sb.AppendLine($"\t[Tag: {Tag}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, Group);
            Helper.WriteString(m, Tag);
            return m.ToArray();
        }
    }
}
