using System;
using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
    public class  RMCPacketRequestGameSessionService_RemoveParticipants : RMCPRequest
    {
        public GameSessionKey Key { get; set; }
        public List<uint> ToRemove { get; set; }
        void PrintHex(Stream s) { var p = s.Position; s.Position = 0; Log.WriteLine(1,BitConverter.ToString(new BinaryReader(s).ReadBytes((int)s.Length)).Replace("-", " ")); s.Position = p; }
        public RMCPacketRequestGameSessionService_RemoveParticipants(Stream s)
        {
            PrintHex(s);
            Key = new GameSessionKey(s);
            ToRemove = new List<uint>();
            uint count = Helper.ReadU32(s);
            for (uint i = 0; i < count; i++)
                ToRemove.Add(Helper.ReadU32(s));
           
        }

        public override string PayloadToString()
        {
            return "";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Key.ToBuffer(m);
            Helper.WriteU32(m, (uint)ToRemove.Count);
            foreach (uint pid in ToRemove)
                Helper.WriteU32(m, pid);
            
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[RemoveParticipants Request]";
        }
    }
}
