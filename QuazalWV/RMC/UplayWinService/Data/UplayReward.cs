using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
    public class UplayReward : IData
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public uint Value { get; set; }
        public string RewardTypeName { get; set; }
        public string GameCode { get; set; }
        public List<UplayRewardPlatform> Platforms { get; set; }

        public UplayReward(string platform)
        {
            RewardTypeName = "Unlockable";
            GameCode = "ACB";
            Platforms = new List<UplayRewardPlatform>() { new UplayRewardPlatform(platform) };
        }

        public UplayReward(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Code = Helper.ReadString(s);
            Name = Helper.ReadString(s);
            Description = Helper.ReadString(s);
            Value = Helper.ReadU32(s);
            RewardTypeName = Helper.ReadString(s);
            GameCode = Helper.ReadString(s);
            Platforms = new List<UplayRewardPlatform>();
            uint count = Helper.ReadU32(s);
            for (uint i = 0; i < count; i++)
                Platforms.Add(new UplayRewardPlatform(s));
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteString(s, Code);
            Helper.WriteString(s, Name);
            Helper.WriteString(s, Description);
            Helper.WriteU32(s, Value);
            Helper.WriteString(s, RewardTypeName);
            Helper.WriteString(s, GameCode);
            Helper.WriteU32(s, (uint)Platforms.Count);
            foreach (UplayRewardPlatform p in Platforms)
                p.ToBuffer(s);
        }
    }
}
