using System.IO;

namespace QuazalWV
{
    internal class RMCPacketRequestFriendsService_AcceptFriendship : RMCPRequest
    {
        public uint Pid { get; set; }
        public uint Details { get; set; }
        
        public RMCPacketRequestFriendsService_AcceptFriendship(Stream s)
        {
            Pid = Helper.ReadU32(s);
        }

        public override string ToString()
        {
            return "[AcceptFriendship Request]";
        }

        public override string PayloadToString()
        {
            return "";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, Pid);
            Helper.WriteU32(m, Details);
            return m.ToArray();
        }
    }
}

