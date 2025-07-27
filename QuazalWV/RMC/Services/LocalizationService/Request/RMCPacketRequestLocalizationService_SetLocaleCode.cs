using System.IO;
using System.Text;

namespace QuazalWV
{
    class RMCPacketRequestLocalizationService_SetLocaleCode : RMCPRequest
    {
        public string LocalCode { get; set; }

        public RMCPacketRequestLocalizationService_SetLocaleCode(Stream s)
        {
            LocalCode = Helper.ReadString(s);
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[LocalCode: {LocalCode}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream result = new MemoryStream();
            Helper.WriteString(result, LocalCode);
            return result.ToArray();
        }

        public override string ToString()
        {
            return "[SetLocaleCode Request]";
        }
    }
}
