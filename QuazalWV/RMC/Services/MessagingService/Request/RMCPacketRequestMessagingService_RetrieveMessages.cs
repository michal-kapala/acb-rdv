using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestMessagingService_RetrieveMessages : RMCPRequest
    {
        public MessageRecipient Recipient { get; set; }
        public List<uint> MessageIds { get; set; } = new List<uint>();
        public bool LeaveOnServer { get; set; }

        public RMCPacketRequestMessagingService_RetrieveMessages(Stream s)
        {
            Recipient = new MessageRecipient(s);
            uint count = Helper.ReadU32(s);
            for (uint i = 0; i < count; i++)
                MessageIds.Add(Helper.ReadU32(s));
            LeaveOnServer = Helper.ReadBool(s);
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Recipient: {Recipient.Pid}]");
            foreach (uint msgId in MessageIds )
                sb.AppendLine($"\t\t[MsgId: {msgId}]");
            sb.AppendLine($"\t[LeaveOnServer: {LeaveOnServer}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Recipient.ToBuffer(m);
            Helper.WriteU32(m, (uint)MessageIds.Count);
            foreach (uint id in MessageIds)
                Helper.WriteU32(m, id);
            Helper.WriteBool(m, LeaveOnServer);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[RetrieveMessages Request]";
        }
    }
}
