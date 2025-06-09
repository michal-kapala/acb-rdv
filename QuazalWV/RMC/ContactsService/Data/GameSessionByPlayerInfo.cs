using System.IO;

namespace QuazalWV
{
    public class GameSessionByPlayerInfo : IData
    {
        public string FriendName {  get; set; }
        public uint FriendPid { get; set; }
        public bool UnkBool1 { get; set; }
        public bool UnkBool2 { get; set; }
        public GameSessionSearchResult SearchResult { get; set; }

        public GameSessionByPlayerInfo(ClientInfo player, GameSessionSearchResult searchResult)
        {
            FriendName = player.User.Name;
            FriendPid = player.User.Pid;
            UnkBool1 = true;
            UnkBool2 = true;
            SearchResult = searchResult;
        }

        public GameSessionByPlayerInfo(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            FriendName = Helper.ReadString(s);
            FriendPid = Helper.ReadU32(s);
            UnkBool1 = Helper.ReadBool(s);
            UnkBool2 = Helper.ReadBool(s);
            SearchResult = new GameSessionSearchResult(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteString(s, FriendName);
            Helper.WriteU32(s, FriendPid);
            Helper.WriteBool(s, UnkBool1);
            Helper.WriteBool(s, UnkBool2);
            SearchResult.ToBuffer(s);
        }
    }
}
