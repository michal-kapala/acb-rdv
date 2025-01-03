using System.IO;

namespace QuazalWV
{
	public class RMCPacketResponseFriendsService_AddFriendByNameWithDetails : RMCPResponse
	{
		public RelationshipData RelationshipData { get; set; }

		public RMCPacketResponseFriendsService_AddFriendByNameWithDetails(uint pid, string name)
		{
			RelationshipData = new RelationshipData
			{
				Pid = pid,
				Name = name,
				ByRelationship = 0,
				Details = 0,
				Status = 0
			};
		}

		public override string PayloadToString()
		{
			return "";
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
