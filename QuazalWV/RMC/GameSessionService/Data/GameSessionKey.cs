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
        public override bool Equals(object obj)
        {
            // Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            GameSessionKey other = (GameSessionKey)obj;
            return TypeId == other.TypeId && SessionId == other.SessionId;
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
            return $"[TypeId: {TypeId}, SessionId: {SessionId}]";
        }
    }
}
