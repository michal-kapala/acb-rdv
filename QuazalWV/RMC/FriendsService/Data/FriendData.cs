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
        public bool InviteNotif { get; set; }

        public FriendData()
        {
            
        }

        public FriendData(Stream s)
        {
            InviteNotif = false;
            FromStream(s);
        }

        public FriendData(Relationship relationship, User otherUser, bool online, bool inviteNotif)
        {
            Pid = otherUser.UserDBPid;
            Name = otherUser.Name;
            Relationship = (byte)relationship.Type;
            Status = online ? "Online" : "Offline";
            Details = relationship.Details;
            InviteNotif = inviteNotif;
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
