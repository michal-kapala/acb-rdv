using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class TrackingService
    {
        public static void ProcessRequest(Stream s,RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestTrackingService_TrackGameSession(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Tracking] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    reply = new RMCPacketResponseTrackingService_TrackGameSession();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Tracking] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
