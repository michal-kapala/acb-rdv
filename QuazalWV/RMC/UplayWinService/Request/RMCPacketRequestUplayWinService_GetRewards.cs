using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestUplayWinService_GetRewards : RMCPRequest
    {
        public uint StartRowIndex { get; set; }
        public uint MaximumRows { get; set; }
        public string SortExpression { get; set; }
        public string CultureName { get; set; }
        public string PlatformCode { get; set; }


        public RMCPacketRequestUplayWinService_GetRewards(Stream s)
        {
            StartRowIndex = Helper.ReadU32(s);
            MaximumRows = Helper.ReadU32(s);
            SortExpression = Helper.ReadString(s);
            CultureName = Helper.ReadString(s);
            PlatformCode = Helper.ReadString(s);
        }

        public override string ToString()
        {
            return "[GetRewards Request]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Culture: {CultureName}]");
            sb.AppendLine($"\t[Platform: {PlatformCode}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, StartRowIndex);
            Helper.WriteU32(m, MaximumRows);
            Helper.WriteString(m, SortExpression);
            Helper.WriteString(m, CultureName);
            Helper.WriteString(m, PlatformCode);
            return m.ToArray();
        }
    }
}
