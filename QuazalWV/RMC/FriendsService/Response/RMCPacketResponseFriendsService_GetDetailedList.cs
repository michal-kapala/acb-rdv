using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
    public class RMCPacketResponseFriendsService_GetDetailedList : RMCPResponse
    {
        public List<FriendData> Friends { get; set; }

        public RMCPacketResponseFriendsService_GetDetailedList(List<FriendData> friends)
        {
            Friends = friends;
        }

        public override string PayloadToString()
        {
            return "";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Friends.Count);
            foreach (FriendData friend in Friends)
                friend.ToBuffer(m);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[GetDetailedList Response]";
        }
    }
}
