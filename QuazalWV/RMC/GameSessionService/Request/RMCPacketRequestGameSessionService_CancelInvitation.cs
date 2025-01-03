using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestGameSessionService_CancelInvitation : RMCPRequest
	{
		public GameSessionInvitationSent InvitationSent { get; set; }

		public RMCPacketRequestGameSessionService_CancelInvitation(Stream s)
		{
			InvitationSent = new GameSessionInvitationSent(s);
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			InvitationSent.ToBuffer(m);
			return m.ToArray();
		}

		public override string ToString()
		{
			return "[CancelInvitation Request]";
		}
	}
}
