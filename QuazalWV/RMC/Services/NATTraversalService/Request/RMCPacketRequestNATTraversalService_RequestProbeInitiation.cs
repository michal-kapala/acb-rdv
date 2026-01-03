using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestNATTraversalService_RequestProbeInitiation : RMCPRequest
    {
        public List<StationUrl> TargetUrls { get; set; }

        public RMCPacketRequestNATTraversalService_RequestProbeInitiation(Stream s)
        {
            HashSet<string> uniqueUrls = new HashSet<string>();
            TargetUrls = new List<StationUrl>();
            var urls = Helper.ReadStringList(s);
            foreach (var url in urls)
            {
                StationUrl parsed = new StationUrl(url);
                ClientInfo owner = null;

                // Prefer PID if present
                if (parsed.PID != 0)
                {
                    owner = Global.Clients.Find(c => c.User.Pid == parsed.PID);
                }

                // Fallback to RVCID
                if (owner == null && parsed.RVCID != 0)
                {
                    owner = Global.Clients.Find(c => c.rvCID == parsed.RVCID);
                }

                // Rewrite endpoint if authoritative owner found
                if (owner != null)
                {
                    parsed.Address = owner.ep.Address.ToString();
                    parsed.Port = (ushort)owner.ep.Port;
                }

                string rewrittenUrl = parsed.ToString();

                // Only add if unique after rewrite
                if (uniqueUrls.Add(rewrittenUrl))
                {
                    TargetUrls.Add(parsed);
                }
            }
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var url in TargetUrls)
                sb.AppendLine($"[URL: {url}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            var urls = new List<string>();
            foreach (var url in TargetUrls)
                urls.Add(url.ToString());
            MemoryStream m = new MemoryStream();
            Helper.WriteStringList(m, urls);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[RequestProbeInitiation Request]";
        }
    }
}
