using System.IO;

namespace QuazalWV
{
    public class TextMessage : IData
    {
        public AnyDataHeader Header { get; set; }
        public uint Id { get; set; }
        public uint RecipientId { get; set; }
        public uint RecipientType { get; set; }
        public uint ParentId { get; set; }
        public uint SenderId { get; set; }
        public QDateTime ReceptionTime {  get; set; }
        public uint Lifetime { get; set; }
        public uint Flags { get; set; }
        public string Subject { get; set; }
        public string SenderName { get; set; }
        public string Body { get; set; }

        public TextMessage()
        {

        }

        public TextMessage(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Header = new AnyDataHeader(s);
            Id = Helper.ReadU32(s);
            RecipientId = Helper.ReadU32(s);
            RecipientType = Helper.ReadU32(s);
            ParentId = Helper.ReadU32(s);
            SenderId = Helper.ReadU32(s);
            ReceptionTime = new QDateTime(s);
            Lifetime = Helper.ReadU32(s);
            Flags = Helper.ReadU32(s);
            Subject = Helper.ReadString(s);
            SenderName = Helper.ReadString(s);
            Body = Helper.ReadString(s);
        }

        public void ToBuffer(Stream s)
        {
            Header.ToBuffer(s);
            Helper.WriteU32(s, Id);
            Helper.WriteU32(s, RecipientId);
            Helper.WriteU32(s, RecipientType);
            Helper.WriteU32(s, ParentId);
            Helper.WriteU32(s, SenderId);
            ReceptionTime.ToBuffer(s);
            Helper.WriteU32(s, Lifetime);
            Helper.WriteU32(s, Flags);
            Helper.WriteString(s, Subject);
            Helper.WriteString(s, SenderName);
            Helper.WriteString(s, Body);
        }

        public UserMessage ToHeader()
        {
            return new UserMessage
            {
                Id = Id,
                RecipientId = RecipientId,
                RecipientType = RecipientType,
                ParentId = ParentId,
                SenderId = SenderId,
                ReceptionTime = new QDateTime(ReceptionTime.RawTime),
                Lifetime = Lifetime,
                Flags = Flags,
                Subject = Subject,
                SenderName = SenderName
            };
        }

        public uint GetSize()
        {
            // Id: 4
            // RecipientId: 4
            // RecipientType: 4
            // ParentId: 4
            // SenderId: 4
            // ReceptionTime: 8
            // LifeTime: 4
            // Flags: 4
            // Subject: 2 (size) + (uint)Subject.Length + 1 (null-terminator)
            // SenderName: 3 + (uint)SenderName.Length
            // Body: 3 + (uint)Body.Length
            return (uint)(45 + Subject.Length + SenderName.Length + Body.Length);
        }
    }
}
