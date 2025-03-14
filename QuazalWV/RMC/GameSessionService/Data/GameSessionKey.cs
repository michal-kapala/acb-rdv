using System.IO;

namespace QuazalWV
{
    public class GameSessionKey : IData
    {
        public uint TypeId { get; set; }
        public uint SessionId { get; set; }

        public GameSessionKey()
        {
            TypeId = 1;
            SessionId = 1;
        }

        public GameSessionKey(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            TypeId = Helper.ReadU32(s);
            SessionId = Helper.ReadU32(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, TypeId);
            Helper.WriteU32(s, SessionId);
        }
        public override string ToString()
        {
            return $"TypeId: {this.TypeId}, SessionId: {this.SessionId}";
        }
    }
}
