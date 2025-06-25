using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseMessagingService_RetrieveMessages : RMCPResponse
    {
        public List<TextMessage> Messages { get; set; } = new List<TextMessage>();

        public RMCPacketResponseMessagingService_RetrieveMessages(List<TextMessage> messages)
        {
            Messages = messages;
        }
        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (TextMessage message in Messages)
                sb.AppendLine($"\t[Msg {message.Id}: {message.Body}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Messages.Count);
            foreach (TextMessage message in Messages)
                message.ToBuffer(m);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[GetMessagesHeaders Response]";
        }
    }
}
