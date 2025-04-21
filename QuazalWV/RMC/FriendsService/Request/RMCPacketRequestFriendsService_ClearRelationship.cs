using System.IO;

namespace QuazalWV
{
    public class RMCPacketRequestFriendsService_ClearRelationship : RMCPRequest
    {
        public uint Pid { get; set; }

        public RMCPacketRequestFriendsService_ClearRelationship(Stream s)
        {
            Pid = Helper.ReadU32(s);
        }

        public override string ToString()
        {
            return "[ClearRelationship Request]";
        }

        public override string PayloadToString()
        {
            return "";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, Pid);
            return m.ToArray();
        }
    }
}
