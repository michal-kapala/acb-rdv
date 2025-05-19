using System.IO;

namespace QuazalWV
{
    public class RMCPacketRequestGameSessionService_DeclineInvitation : RMCPRequest
    {
        public GameSessionInvitationReceived InvitationRecv { get; set; }

        public RMCPacketRequestGameSessionService_DeclineInvitation(Stream s)
        {
            InvitationRecv = new GameSessionInvitationReceived(s);
        }

        public override string PayloadToString()
        {
            return InvitationRecv.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            InvitationRecv.ToBuffer(m);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[DeclineInvitation Request]";
        }
    }
}
