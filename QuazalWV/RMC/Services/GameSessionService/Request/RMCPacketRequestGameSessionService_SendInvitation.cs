using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestGameSessionService_SendInvitation : RMCPRequest
	{
		public GameSessionInvitation Invitation { get; set; }

		public RMCPacketRequestGameSessionService_SendInvitation(Stream s)
		{
			Invitation = new GameSessionInvitation(s);
		}

		public override string ToString()
		{
			return "[SendInvitation Request]";
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Invitation.ToBuffer(m);
			return m.ToArray();
		}
	}
}
