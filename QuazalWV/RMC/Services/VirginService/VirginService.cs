using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public class VirginService 
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.Virgin;

        public static void ProcessRequest(Stream s, RMCP rmc, ClientInfo client)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestVirginService_SendVirginInfo(s);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }

        public static void HandleRequest(PrudpPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    var reqSendVinfo = (RMCPacketRequestVirginService_SendVirginInfo)rmc.request;
                    Log.WriteRmcLine(1, $"VirginInfo: {reqSendVinfo.PayloadToString()}", protocol, LogSource.RMC, Color.Black, client);
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
