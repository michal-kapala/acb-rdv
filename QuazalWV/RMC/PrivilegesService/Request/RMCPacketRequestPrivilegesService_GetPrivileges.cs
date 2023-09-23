using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestPrivilegesService_GetPrivileges : RMCPRequest
    {
        public string LocaleCode { get; set; }

        public RMCPacketRequestPrivilegesService_GetPrivileges(Stream s)
        {
            LocaleCode = Helper.ReadString(s);
        }

        public override string ToString()
        {
            return $"[GetPrivileges Request, locale={LocaleCode}]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Locale: {LocaleCode}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteString(m, LocaleCode);
            return m.ToArray();
        }
    }
}
