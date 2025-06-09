using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestContactsService_RetrieveGameSessionFromContact : RMCPRequest
    {
        public GameType SessionType { get; set; }
        public List<string> FriendNames { get; set; }

        public RMCPacketRequestContactsService_RetrieveGameSessionFromContact(Stream s)
        {
            SessionType = (GameType)Helper.ReadU32(s);
            FriendNames = new List<string>();
            uint count = Helper.ReadU32(s);
            for (uint i = 0; i < count; i++)
                FriendNames.Add(Helper.ReadString(s));
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[SessionType: {(uint)SessionType} ({SessionType})]");
            foreach (string s in FriendNames)
                sb.AppendLine($"[{s}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)SessionType);
            Helper.WriteU32(m, (uint)FriendNames.Count);
            foreach (string s in FriendNames)
                Helper.WriteString(m, s);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[RetrieveGameSessionFromContact Request]";
        }
    }
}
