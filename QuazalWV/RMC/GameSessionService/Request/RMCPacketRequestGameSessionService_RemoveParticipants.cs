using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
    public class  RMCPacketRequestGameSessionService_RemoveParticipants : RMCPRequest
    {
        public GameSessionKey Key { get; set; }
        public List<uint> PublicPids { get; set; }
        public List<uint> PrivatePids { get; set; }

        public RMCPacketRequestGameSessionService_RemoveParticipants(Stream s)
        {
            Key = new GameSessionKey(s);
            PublicPids = new List<uint>();
            uint count = Helper.ReadU32(s);
            for (uint i = 0; i < count; i++)
                PublicPids.Add(Helper.ReadU32(s));
;
        }

        public override string PayloadToString()
        {
            return "";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Key.ToBuffer(m);
            Helper.WriteU32(m, (uint)PublicPids.Count);
            foreach (uint pid in PublicPids)
                Helper.WriteU32(m, pid);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[RemoveParticipants Request]";
        }
    }
}
