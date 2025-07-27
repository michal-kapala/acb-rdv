using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestFriendsService_AddFriendByNameWithDetails : RMCPRequest
	{
		public string Invitee {  get; set; }
		public uint Details { get; set; }
		public string Inviter { get; set; }

		public RMCPacketRequestFriendsService_AddFriendByNameWithDetails(Stream s)
		{
			Invitee = Helper.ReadString(s);
			Details = Helper.ReadU32(s);
			Inviter = Helper.ReadString(s);
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteString(m, Invitee);
			Helper.WriteU32(m, Details);
			Helper.WriteString(m, Inviter);
			return m.ToArray();
		}

		public override string ToString()
		{
			return "[AddFriendByNameWithDetails Request]";
		}
	}
}
