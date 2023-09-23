using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuazalWV
{
    public class RMCPacketResponseUplayWinService_GetRewards : RMCPResponse
    {
        public List<UplayReward> Rewards { get; set; }
        public RMCPacketResponseUplayWinService_GetRewards(List<UplayReward> rewards)
        {
            Rewards = rewards;
        }

        public override string ToString()
        {
            return "[GetRewards Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(UplayReward reward in Rewards)
                sb.AppendLine($"\t[Reward: {reward.Name}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Rewards.Count);
            foreach (UplayReward reward in Rewards)
                reward.ToBuffer(m);
                
            return m.ToArray();
        }
    }
}
