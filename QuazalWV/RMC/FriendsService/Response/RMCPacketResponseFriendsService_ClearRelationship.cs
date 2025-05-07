using System.IO;

namespace QuazalWV
{
	public class RMCPacketResponseFriendsService_ClearRelationship : RMCPResponse
	{
		public bool RetVal { get; set; }

		public RMCPacketResponseFriendsService_ClearRelationship(bool result)
		{
			RetVal = result;
		}

		public override string ToString()
		{
			return "[ClearRelationship Response]";
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
