using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public class VirginService 
    {
        public static void ProcessRequest(Stream s, RMCP rmc, ClientInfo client)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestVirginService_SendVirginInfo(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Virgin] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    var reqSendVinfo = (RMCPacketRequestVirginService_SendVirginInfo)rmc.request;
                    Log.WriteLine(1, $"[RMC Virgin] VirginInfo: {reqSendVinfo.PayloadToString()}", Color.Black, client);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                
                default:
                    Log.WriteLine(1, $"[RMC Virgin] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
