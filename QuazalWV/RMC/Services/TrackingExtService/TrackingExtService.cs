using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public static class TrackingExtService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestTrackingExtService_IsTrackingSessionActive(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Tracking Ext] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(PrudpPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    reply = new RMCPacketResponseTrackingExtService_IsTrackingSessionActive();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Tracking Ext] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
