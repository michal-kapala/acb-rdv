using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class NATTraversalService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.NatTraversalRelay;

        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestNATTraversalService_RequestProbeInitiation(s);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red);
                    break;
            }
        }

        public static void HandleRequest(PrudpPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    var reqReqProbeInit = (RMCPacketRequestNATTraversalService_RequestProbeInitiation)rmc.request;
                    RMCPacketRequestNATTraversalService_InitiateProbe reqInitProbe;
                    foreach (var url in reqReqProbeInit.TargetUrls)
                    {
                        Log.WriteLine(1, $"[NAT url: {url}]", LogSource.StationURL, Color.HotPink, client);
                        var player = Global.Clients.Find(c => c.User.Pid == url.PID);
                        // NAT relay
                        if (player != null)
                        {
                            var targetUrl = new StationUrl(client);
                            reqInitProbe = new RMCPacketRequestNATTraversalService_InitiateProbe(targetUrl);
                            RMC.SendRequest(player, reqInitProbe, RMCP.PROTOCOL.NatTraversalRelay, 2);
                        }
                    }
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }
    }
}
