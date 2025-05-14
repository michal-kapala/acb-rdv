using System.IO;

namespace QuazalWV
{
    public class FriendData : IData
    {
        public uint Pid { get; set; }
        public string Name { get; set; }
        public byte Relationship { get; set; }
        public uint Details { get; set; }
        public string Status { get; set; }

        public FriendData()
        {
            
        }

        public FriendData(Stream s)
        {
            FromStream(s);
        }

        public FriendData(Relationship relationship, User otherUser, bool online)
        {
            Pid = otherUser.Pid;
            Name = otherUser.Name;
            switch(relationship.Type)
            {
                case DbPlayerRelationshipType.Pending:
                    Relationship = otherUser.Pid == relationship.RequesterPid ? (byte)PlayerRelationship.PendingIn : (byte)PlayerRelationship.PendingOut;
                    Status = "";
                    break;
                case DbPlayerRelationshipType.Blocked:
                    Relationship = (byte)PlayerRelationship.Blocked;
                    Status = "";
                    break;
                case DbPlayerRelationshipType.Friends:
                    Relationship = (byte)PlayerRelationship.Friend;
                    Status = online ? "Online" : "Offline";
                    break;
            }
            Details = relationship.Details;
        }

        public void FromStream(Stream s)
        {
            Pid = Helper.ReadU32(s);
            Name = Helper.ReadString(s);
            Relationship = Helper.ReadU8(s);
            Details = Helper.ReadU32(s);
            Status = Helper.ReadString(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, Pid);
            Helper.WriteString(s, Name);
            Helper.WriteU8(s, Relationship);
            Helper.WriteU32(s, Details);
            Helper.WriteString(s, Status);
        }
    }
}
