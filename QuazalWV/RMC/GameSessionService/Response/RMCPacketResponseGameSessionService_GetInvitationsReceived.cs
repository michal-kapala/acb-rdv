using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseGameSessionService_GetInvitationsReceived : RMCPResponse
    {
        public List<GameSessionInvitationReceived> Invitations { get; set; }

        public RMCPacketResponseGameSessionService_GetInvitationsReceived()
        {
            Invitations = new List<GameSessionInvitationReceived>();
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Invite count: {Invitations.Count}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Invitations.Count);
            foreach (GameSessionInvitationReceived i in Invitations)
                i.ToBuffer(m);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[GetInvitationsReceived Response]";
        }
    }
}
