using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestNewsService_GetNumberOfNews : RMCPRequest
    {
        public NewsRecipient Recipient { get; set; }

        public RMCPacketRequestNewsService_GetNumberOfNews(Stream s)
        {
            Recipient = new NewsRecipient(s);
        }

        public override string ToString()
        {
            return "[GetNumberOfNews Request]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[Recipient ID: {Recipient.Id}]");
            sb.AppendLine($"[Recipient type: {Recipient.Type}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Recipient.ToBuffer(m);
            return m.ToArray();
        }
    }
}
