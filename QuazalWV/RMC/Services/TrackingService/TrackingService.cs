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
                    rmc.request = new RMCPacketRequestTrackingService_SendTag(s);
                    break;
                case 3:
                    rmc.request = new RMCPacketRequestTrackingService_SendUserInfo(s);
                    break;
                case 4:
                    // Empty GetConfiguration request
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
                    var rSendTag = (RMCPacketRequestTrackingService_SendTag)rmc.request;
                    var tag = new TelemetryTag()
                    {
                        TrackingId = rSendTag.TrackingId,
                        Tag = rSendTag.Tag,
                        Attributes = rSendTag.Attributes,
                        DeltaTime = rSendTag.DeltaTime
                    };
                    DbHelper.SaveTag(tag);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 3:
                    reply = new RMCPacketResponseTrackingService_SendUserInfo(client);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 4:
                    var tags = DbHelper.GetTags();
                    reply = new RMCPacketResponseTrackingService_GetConfiguration(tags);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Tracking] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
