using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseMessagingService_GetMessagesHeaders : RMCPResponse
    {
        public List<UserMessage> Messages { get; set; }

        public RMCPacketResponseMessagingService_GetMessagesHeaders(List<UserMessage> headers)
        {
            Messages = headers;
        }

        public override string ToString()
        {
            return "[GetMessagesHeaders Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Messages: {Messages.Count}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Messages.Count);
            foreach (var msg in Messages)
                msg.ToBuffer(m);
            return m.ToArray();
        }
    }
}
