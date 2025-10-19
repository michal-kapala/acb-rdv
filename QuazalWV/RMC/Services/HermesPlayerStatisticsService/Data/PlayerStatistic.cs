using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
    public class PlayerStatistic : IData
    {
        public uint Id { get; set; }
        public List<PlayerStatisticInfo> Infos { get; set; }

        public PlayerStatistic(StatQuery query)
        {
            Id = 0;
            Infos = new List<PlayerStatisticInfo>();
            foreach (uint id in query.InfoIds)
                Infos.Add(new PlayerStatisticInfo((byte)id, (byte)Id));
        }

        public PlayerStatistic(Stream s)
        {
            Infos = new List<PlayerStatisticInfo>();
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Id = Helper.ReadU32(s);
            uint count = Helper.ReadU32(s);
            for (uint i = 0; i < count; i++)
                Infos.Add(new PlayerStatisticInfo(s));
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU32(s, Id);
            Helper.WriteU32(s, (uint)Infos.Count);
            foreach (PlayerStatisticInfo info in Infos)
                info.ToBuffer(s);
        }
    }
}
