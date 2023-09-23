using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestFriendsService_GetRelationships : RMCPRequest
    {
        public ResultRange ResultRange { get; set; }

        public RMCPacketRequestFriendsService_GetRelationships(Stream s)
        {
            ResultRange = new ResultRange(s);
        }

        public override string ToString()
        {
            return "[GetRelationships Request]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Offset: {ResultRange.Offset}]");
            sb.AppendLine($"\t[Size: {ResultRange.Size}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            ResultRange.ToBuffer(m);
            return m.ToArray();
        }
    }
}
