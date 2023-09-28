using System.IO;
using System.Linq;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseSecureService_Register : RMCPResponse
    {
        public const uint ResultCode = 0x10001;
        public const uint ConnectionId = 78;
        public string ClientUrl { get; set; }

        public RMCPacketResponseSecureService_Register(ClientInfo client)
        {
            // All the urls have the ISP's endpoint set already
            StationUrl url = new StationUrl(client.TrackingUserUrls.First())
            {
                SID = 14,
                Type = 3
            };
            ClientUrl = url.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, ResultCode);
            Helper.WriteU32(m, ConnectionId);
            Helper.WriteString(m, ClientUrl);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[Register Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\t[Result Code   : 0x" + ResultCode.ToString("X8") + "]");
            sb.AppendLine("\t[Connection Id : 0x" + ConnectionId.ToString("X8") + "]");
            sb.AppendLine("\t[Client Url    : " + ClientUrl + "]");
            return sb.ToString();
        }
    }
}