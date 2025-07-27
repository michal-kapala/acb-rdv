using System.IO;

namespace QuazalWV
{
    public class UplayRewardPlatform : IData
    {
        public string PlatformCode { get; set; }
        public bool Purchased { get; set; }

        public UplayRewardPlatform(string platform)
        {
            PlatformCode = platform;
            Purchased = true;
        }

        public UplayRewardPlatform(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            PlatformCode = Helper.ReadString(s);
            Purchased = Helper.ReadBool(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteString(s, PlatformCode);
            Helper.WriteBool(s, Purchased);
        }
    }
}
