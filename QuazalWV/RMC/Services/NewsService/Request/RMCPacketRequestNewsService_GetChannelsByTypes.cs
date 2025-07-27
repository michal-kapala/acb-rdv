using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestNewsService_GetChannelsByTypes : RMCPRequest
    {
        public List<string> NewsChannelTypes { get; set; }
        public ResultRange ResultRange { get; set; }


        public RMCPacketRequestNewsService_GetChannelsByTypes(Stream s)
        {
            NewsChannelTypes = new List<string>();
            uint count = Helper.ReadU32(s);
            for (int i = 0; i < count; i++)
                NewsChannelTypes.Add(Helper.ReadString(s));
            ResultRange = new ResultRange(s);
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(string type in NewsChannelTypes)
                sb.AppendLine($"\t[Type: {type}]");
            sb.AppendLine($"\t[Max channels: {ResultRange.Size}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)NewsChannelTypes.Count);
            foreach (string type in NewsChannelTypes)
                Helper.WriteString(m, type);
            ResultRange.ToBuffer(m);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[GetChannelsByTypes Request]";
        }
    }
}
