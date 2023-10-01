using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestMessagingService_GetMessagesHeaders : RMCPRequest
    {
        public MessageRecipient Recipient { get; set; }
        public ResultRange Range { get; set; }

        public RMCPacketRequestMessagingService_GetMessagesHeaders(Stream s)
        {
            Recipient = new MessageRecipient(s);
            Range = new ResultRange(s);
        }

        public override string ToString()
        {
            return "[GetMessagesHeaders Request]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Recipient: {Recipient.Pid}]");
            sb.AppendLine($"\t[Gathering: {Recipient.GatheringId}]");
            sb.AppendLine($"\t[Max messages: {Range.Size}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Recipient.ToBuffer(m);
            Range.ToBuffer(m);
            return m.ToArray();
        }
    }
}
