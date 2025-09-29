using System.Collections.Generic;
using System.IO;
using System.Text;
namespace QuazalWV
{
    public class RMCPacketRequestGameSessionService_RegisterURLs : RMCPRequest
    {
        public List<StationUrl> Urls { get; set; }
        public RMCPacketRequestGameSessionService_RegisterURLs(Stream s, ClientInfo client)
        {
            Urls = new List<StationUrl>();
            uint count = Helper.ReadU32(s);
            for (uint i = 0; i < count; i++)
            {
                string b = Helper.ReadString(s);
                // Create StationUrl of the string
                var url = new StationUrl(b);

                // Check if the IP is local (private IP ranges)
                if (IsLocalIp(url.Address))
                {
                    // Replace with the UDP endpoint IP of the client (public IP / NAT IP)
                    url.Address = client.ep.Address.ToString();
                }

                Urls.Add(url);

                Log.WriteRmcLine(1, $"RegisterURLs - host URL: {url}", RMCP.PROTOCOL.GameSession, LogSource.RMC);
            }
        }

        private bool IsLocalIp(string ip)
        {
            return ip.StartsWith("192.168.") || ip.StartsWith("10.") ||
                   (ip.StartsWith("172.") && IsInRange172(ip));
        }

        private bool IsInRange172(string ip)
        {
            // 172.16.0.0 – 172.31.255.255
            string[] parts = ip.Split('.');
            if (parts.Length != 4) return false;
            if (int.TryParse(parts[1], out int secondOctet))
                return secondOctet >= 16 && secondOctet <= 31;
            return false;
        }

        public override string ToString()
        {
            return "[RegisterURLs Request]";
        }
        public override string PayloadToString()
        {
            var sb = new StringBuilder();
            foreach (StationUrl url in Urls)
                sb.Append($"\t[{url}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Urls.Count);
            foreach (StationUrl url in Urls)
                Helper.WriteString(m, url.ToString());
            return m.ToArray();
        }

        public void RegisterUrls(ClientInfo client, Session ses)
        {
            client.RegisteredUrls.Clear();
            ses.HostUrls.Clear();
            foreach (StationUrl url in Urls)
            {
                client.RegisteredUrls.Add(url);
                ses.HostUrls.Add(url);
            }
        }
    }
}
