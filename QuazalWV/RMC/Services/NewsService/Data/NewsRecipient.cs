using System.IO;

namespace QuazalWV
{
    public class NewsRecipient : IData
    {
        public uint Id { get; set; }
        public uint Type { get; set; }

        public NewsRecipient(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Id = Helper.ReadU32(s);
            Type = Helper.ReadU32(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, Id);
            Helper.WriteU32(s, Type);
        }
    }
}
