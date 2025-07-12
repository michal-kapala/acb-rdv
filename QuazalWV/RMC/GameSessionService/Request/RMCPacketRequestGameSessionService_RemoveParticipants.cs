using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
    public class  RMCPacketRequestGameSessionService_RemoveParticipants : RMCPRequest
    {
        public GameSessionKey Key { get; set; }
        public List<uint> Pids { get; set; }

        public RMCPacketRequestGameSessionService_RemoveParticipants(Stream s)
        {
            Key = new GameSessionKey(s);
            Pids = new List<uint>();
            uint count = Helper.ReadU32(s);
            for (uint i = 0; i < count; i++)
                Pids.Add(Helper.ReadU32(s));
        }

        public override string PayloadToString()
        {
            return "";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Key.ToBuffer(m);
            Helper.WriteU32(m, (uint)Pids.Count);
            foreach (uint pid in Pids)
                Helper.WriteU32(m, pid);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[RemoveParticipants Request]";
        }
    }
}
