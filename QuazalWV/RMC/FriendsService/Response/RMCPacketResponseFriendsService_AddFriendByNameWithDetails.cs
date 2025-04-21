using System.IO;

namespace QuazalWV
{
    public class RMCPacketResponseFriendsService_AddFriendByNameWithDetails : RMCPResponse
    {
        public uint statustryout = 0;
        public RelationshipData RelationshipData { get; set; }

        public RMCPacketResponseFriendsService_AddFriendByNameWithDetails(uint pid, string name, bool add_result)
        {
            RelationshipData = new RelationshipData
            {
                Pid = pid,
                Name = name,
                ByRelationship = 0,
                Details = statustryout,
                Status = (byte)statustryout
            };
            Log.WriteLine(1, $"increased log {statustryout}");
            statustryout += 1;

        }

        public override string PayloadToString()
        {
            return " ";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            RelationshipData.ToBuffer(m);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[AddFriendByNameWithDetails Response]";
        }
    }
}
