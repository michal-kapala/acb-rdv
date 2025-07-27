using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseGameInfoService_GetFileListOnGameStorage : RMCPResponse
    {
        public List<QFile> Files { get; set; }

        public RMCPacketResponseGameInfoService_GetFileListOnGameStorage(string fileName)
        {
            Files = new List<QFile>() { new QFile(fileName) };
        }

        public override string ToString()
        {
            return "[GetFileListOnGameStorage Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Files: {Files.Count}]");
            foreach(QFile f in Files)
                sb.AppendLine($"\t\t[Name: {f.Name}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Files.Count);
            foreach (QFile f in Files)
                f.ToBuffer(m);
            return m.ToArray();
        }
    }
}
