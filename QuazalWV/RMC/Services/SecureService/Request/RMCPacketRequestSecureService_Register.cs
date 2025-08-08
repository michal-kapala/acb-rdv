using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestSecureService_Register : RMCPRequest
    {
        public List<string> StationUrls { get; set; }

        public RMCPacketRequestSecureService_Register(Stream s)
        {
            StationUrls = Helper.ReadStringList(s);
        }

        public override byte[] ToBuffer()
        {
            MemoryStream result = new MemoryStream();
            Helper.WriteStringList(result, StationUrls);
            return result.ToArray();
        }

        public override string ToString()
        {
            return "[Register Request]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\t[Station List :]");
            foreach (string s in StationUrls)
                sb.AppendLine("\t\t\t[\"" + s + "\"]");
            return sb.ToString();
        }

        public void SetUrls(ClientInfo client)
        {
            List<string> trackingUrls = new List<string>();
            // Update local endpoints to as-seen by the server (the same/public ones for LAN/online server)
            foreach (string url in StationUrls)
            {
                StationUrl sUrl = new StationUrl(url)
                {
                    //Address = client.ep.Address.ToString(),
                };
                // Add to the updated list
                trackingUrls.Add(sUrl.ToString());
            }
            if (trackingUrls.Count == 0)
                Log.WriteRmcLine(1, $"Register: assigned empty URL list", RMCP.PROTOCOL.Secure, LogSource.RMC, Color.Red, client);
            
            client.TrackingUserUrls = trackingUrls;

            // Just-in-case update for the global list
            ClientInfo listClient = Global.Clients.Find(c => c.User.Pid == client.User.Pid);
            if (listClient == null)
            {
                Log.WriteRmcLine(1, "Register: the client wasn't found in global list, adding", RMCP.PROTOCOL.Secure, LogSource.RMC, Color.Red, client);
                Global.Clients.Add(client);
            }
        }
    }
}