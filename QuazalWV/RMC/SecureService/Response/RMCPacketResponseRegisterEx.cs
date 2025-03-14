using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseRegisterEx : RMCPResponse
    {
        public uint resultCode = 0x00010001;
        public uint connectionId = 79;
        public string clientUrl;
        public RMCPacketResponseRegisterEx(ClientInfo client)
        {
            this.connectionId = client.RVCid;
            clientUrl = $"prudps:/address={client.ep.Address};port={client.ep.Port};sid=15;type=3";
            Log.WriteLine(1, $"[RMCPacketResponseRegisterEx] url_REGISTERING: ${clientUrl}", System.Drawing.Color.Red);
            client.Urls.Add(new StationUrl(clientUrl));
        }

        public override byte[] ToBuffer()
        {
            
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, resultCode);
            Helper.WriteU32(m, connectionId);
            Helper.WriteString(m, clientUrl);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[RegisterEx Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\t[Result Code   : 0x" + resultCode.ToString("X8") + "]");
            sb.AppendLine("\t[Connection Id : 0x" + connectionId.ToString("X8") + "]");
            sb.AppendLine("\t[Client Url    : " + clientUrl + "]");
            return sb.ToString();
        }
    }
}