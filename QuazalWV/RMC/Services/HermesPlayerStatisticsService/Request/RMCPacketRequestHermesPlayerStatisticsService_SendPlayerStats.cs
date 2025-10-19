using System;
using System.IO;

namespace QuazalWV
{
    internal class RMCPacketRequestHermesPlayerStatisticsService_SendPlayerStats : RMCPRequest
    {
        private Stream s;

        public RMCPacketRequestHermesPlayerStatisticsService_SendPlayerStats(Stream s)
        {
            UInt32 count = Helper.ReadU32(s);
            for (int i = 0; i < count; i++)
            {
                PlayerStatistic playerStats = new PlayerStatistic(s);
            }
        }
        public override string PayloadToString()
        {
            return "";
        }
        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[SendPlayerStats Request]";
        }
    }
}