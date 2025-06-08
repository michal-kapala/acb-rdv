using System.IO;

namespace QuazalWV
{
    public class GameSessionByPlayerInfo : IData
    {
        public string UnkString {  get; set; }
        public uint UnkUint { get; set; }
        public bool UnkBool1 { get; set; }
        public bool UnkBool2 { get; set; }
        public GameSessionSearchResult SearchResult { get; set; }

        public GameSessionByPlayerInfo()
        {

        }

        public GameSessionByPlayerInfo(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            UnkString = Helper.ReadString(s);
            UnkUint = Helper.ReadU32(s);
            UnkBool1 = Helper.ReadBool(s);
            UnkBool2 = Helper.ReadBool(s);
            SearchResult = new GameSessionSearchResult(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteString(s, UnkString);
            Helper.WriteU32(s, UnkUint);
            Helper.WriteBool(s, UnkBool1);
            Helper.WriteBool(s, UnkBool2);
            SearchResult.ToBuffer(s);
        }
    }
}
