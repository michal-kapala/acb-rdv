using System.IO;

namespace QuazalWV
{
    public class MessageRecipient : IData
    {
        // RecipientType field does not exist in ACB
        public uint Pid { get; set; }
        public uint GatheringId { get; set; }

        public MessageRecipient()
        {
            
        }

        public MessageRecipient(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Pid = Helper.ReadU32(s);
            GatheringId = Helper.ReadU32(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, Pid);
            Helper.WriteU32(s, GatheringId);
        }
    }
}
