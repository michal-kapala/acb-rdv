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

                url.Address = client.ep.Address.ToString();

                Urls.Add(url);

                Log.WriteRmcLine(1, $"RegisterURLs - host URL: {url}", RMCP.PROTOCOL.GameSession, LogSource.RMC);
            }
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
