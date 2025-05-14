using System.IO;

namespace QuazalWV
{
    internal class RMCPacketRequestGameSessionService_AcceptInvitation : RMCPRequest
    {
        private Stream s;

		public GameSessionInvitationReceived InvitationReceived { get; set; }

		public RMCPacketRequestGameSessionService_AcceptInvitation(Stream s)
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