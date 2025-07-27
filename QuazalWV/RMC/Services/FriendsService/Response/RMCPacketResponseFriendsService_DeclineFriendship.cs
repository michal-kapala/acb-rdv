using System.IO;

namespace QuazalWV
{
    internal class RMCPacketResponseFriendsService_DeclineFriendship : RMCPResponse
    {
        public bool RetVal { get; set; }

        public RMCPacketResponseFriendsService_DeclineFriendship(bool retval)
        {
            RetVal = retval;
        }

        public override string ToString()
        {
            return "[DeclineFriendship Response]";
        }

        public override string PayloadToString()
        {
            return "";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteBool(m, RetVal);
            return m.ToArray();
        }
    }
}
