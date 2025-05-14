using System.IO;

namespace QuazalWV
{
    internal class RMCPacketRequestGameSessionService_DenyInvitation : RMCPRequest
    {
		public GameSessionInvitationReceived InvitationReceived { get; set; }

		public RMCPacketRequestGameSessionService_DenyInvitation(Stream s)
		{
			InvitationReceived = new GameSessionInvitationReceived(s);
		}

		public override string PayloadToString()
		{
			return $"{InvitationReceived.ToString()}";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			InvitationReceived.ToBuffer(m);
			return m.ToArray();
		}

		public override string ToString()
		{
			return $"[CancelInvitation Request] {InvitationReceived.ToString()}";
		}
	}
}