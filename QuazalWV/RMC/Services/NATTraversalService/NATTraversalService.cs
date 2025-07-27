using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class NATTraversalService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestNATTraversalService_RequestProbeInitiation(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC NAT] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    var reqReqProbeInit = (RMCPacketRequestNATTraversalService_RequestProbeInitiation)rmc.request;
                    RMCPacketRequestNATTraversalService_InitiateProbe reqInitProbe;
                    foreach (var url in reqReqProbeInit.TargetUrls)
                    {
                        Log.WriteLine(1, $"[NAT url: {url}]", Color.Pink, client);
                        var player = Global.Clients.Find(c => c.User.Pid == url.PID);
                        // NAT relay
                        if (player != null)
                        {
                            var targetUrl = new StationUrl(client);
                            reqInitProbe = new RMCPacketRequestNATTraversalService_InitiateProbe(targetUrl);
                            RMC.SendRequest(player, reqInitProbe, RMCP.PROTOCOL.NATTraversalRelayService, 2);
                        }
                    }
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC NAT] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
