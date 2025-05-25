using System.IO;

namespace QuazalWV
{
    public class RelationshipData : IData
    {
        public uint Pid { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Defines friend's current status (PlayerRelationship).
        /// </summary>
        public byte ByRelationship { get; set; }
        public uint Details { get; set; }
        public byte Status { get; set; }

        public RelationshipData()
        {
            Details = 0;
            Status = 0;
        }

        public RelationshipData(Stream s)
        {
            FromStream(s);
        }

        public RelationshipData(Relationship relationship, User otherUser, bool online)
        {
            Pid = otherUser.UserDBPid;
            Name = otherUser.Name;
            ByRelationship = (byte)relationship.Type;
            Details = relationship.Details;
            // TODO: check status values
            Status = (byte)(online ? 1 : 0);
        }

        public void FromStream(Stream s)
        {
            Pid = Helper.ReadU32(s);
            Name = Helper.ReadString(s);
            ByRelationship = Helper.ReadU8(s);
            Details = Helper.ReadU32(s);
            Status = Helper.ReadU8(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, Pid);
            Helper.WriteString(s, Name);
            Helper.WriteU8(s, ByRelationship);
            Helper.WriteU32(s, Details);
            Helper.WriteU8(s, Status);
        }
    }
}
