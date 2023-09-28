using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestGameInfoService_GetFileListOnGameStorage : RMCPRequest
    {
        public uint MinNbElements { get; set; }
        public uint MaxNbElements { get; set; }
        public string FileName { get; set; }

        public RMCPacketRequestGameInfoService_GetFileListOnGameStorage(Stream s)
        {
            MinNbElements = Helper.ReadU32(s);
            MaxNbElements = Helper.ReadU32(s);
            FileName = Helper.ReadString(s);
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Min elements: {MinNbElements}]");
            sb.AppendLine($"\t[Max elements: {MaxNbElements}]");
            sb.AppendLine($"\t[File: {FileName}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, MinNbElements);
            Helper.WriteU32(m, MaxNbElements);
            Helper.WriteString(m, FileName);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[GetFileListOnGameStorage Request]";
        }
    }
}
