using System.IO;

namespace QuazalWV
{
    public class UserMessage : IData
    {
        public uint Id { get; set; }
        public uint ParentId { get; set; }
        public uint PidSender { get; set; }
        public MessageRecipient Recipient { get; set; }
        public QDateTime ReceptionTime { get; set; }
        public uint Lifetime { get; set; }
        public uint Flags { get; set; }
        public string Subject { get; set; }
        public string Sender { get; set; }

        public UserMessage()
        {

        }

        public UserMessage(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Id = Helper.ReadU32(s);
            ParentId = Helper.ReadU32(s);
            PidSender = Helper.ReadU32(s);
            Recipient = new MessageRecipient(s);
            ReceptionTime = new QDateTime(s);
            Lifetime = Helper.ReadU32(s);
            Flags = Helper.ReadU32(s);
            Subject = Helper.ReadString(s);
            Sender = Helper.ReadString(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, Id);
            Helper.WriteU32(s, ParentId);
            Helper.WriteU32(s, PidSender);
            Recipient.ToBuffer(s);
            ReceptionTime.ToBuffer(s);
            Helper.WriteU32(s, Lifetime);
            Helper.WriteU32(s, Flags);
            Helper.WriteString(s, Subject);
            Helper.WriteString(s, Sender);
        }
    }
}
