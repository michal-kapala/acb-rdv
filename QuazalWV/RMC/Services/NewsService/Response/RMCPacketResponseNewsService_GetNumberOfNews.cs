using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseNewsService_GetNumberOfNews : RMCPResponse
    {
        public uint NbOfNews { get; set; }

        public RMCPacketResponseNewsService_GetNumberOfNews()
        {
            NbOfNews = 0;
        }

        public override string ToString()
        {
            return "[GetNumberOfNews Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[News: {NbOfNews}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, NbOfNews);
            return m.ToArray();
        }
    }
}
