using System.IO;
using System.Drawing;

namespace QuazalWV
{
    public static class SecureService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.Secure;

        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestSecureService_Register(s);
                    break;
                case 4:
                    rmc.request = new RMCPacketRequestSecureService_RegisterEx(s);
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
                    var reqRegister = (RMCPacketRequestSecureService_Register)rmc.request;
                    // Change ip addresses to external and save urls
                    reqRegister.SetUrls(client);
                    reply = new RMCPacketResponseSecureService_Register(client);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 4:
                    var reqRegisterEx = (RMCPacketRequestSecureService_RegisterEx)rmc.request;
                    switch (reqRegisterEx.className)
                    {
                        case "UbiAuthenticationLoginCustomData":
                            foreach (var url in reqRegisterEx.stationUrls)
                                client.Urls.Add(new StationUrl(url));
                            reply = new RMCPacketResponseSecureService_RegisterEx(client);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                            break;
                        default:
                            Log.WriteRmcLine(1, $"Error: Unknown Custom Data class {reqRegisterEx.className}", protocol, LogSource.RMC, Color.Red, client);
                            break;
                    }
                    break;
                default:
                    Log.WriteRmcLine(1, $"Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }
    }
}
