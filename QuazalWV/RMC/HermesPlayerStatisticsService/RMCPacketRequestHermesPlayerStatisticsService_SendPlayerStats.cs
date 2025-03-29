using System.IO;

namespace QuazalWV
{
    internal class RMCPacketRequestHermesPlayerStatisticsService_SendPlayerStats : RMCPRequest
    {
        private Stream s;

        public RMCPacketRequestHermesPlayerStatisticsService_SendPlayerStats(Stream s)
        {
            this.s = s;
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