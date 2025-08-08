using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class RichPresenceService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestRichPresenceService_SetPresence(s);
                    break;
                case 2:
                    rmc.request = new RMCPacketRequestRichPresenceService_GetPresence(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC RichPresence] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(PrudpPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    var reqSetPresence = (RMCPacketRequestRichPresenceService_SetPresence)rmc.request;
                    client.PresenceProps = reqSetPresence.Props;
                    foreach (var prop in reqSetPresence.Props)
                        Log.WriteLine(2, $"[RMC RichPresence] {prop}", Color.Blue, client);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 2:
                    var reqGetPresence = (RMCPacketRequestRichPresenceService_GetPresence)rmc.request;
                    var presenceElements = new List<PresenceElement>();
                    ClientInfo friend;
                    PresenceElement elem;
                    List<PresenceProperty> props;
                    byte[] serializedProps;
                    foreach (uint pid in reqGetPresence.Pids)
                    {
                        friend = Global.Clients.Find(c => c.User.Pid == pid);
                        props = friend == null ? new List<PresenceProperty>() : friend.PresenceProps;
                        foreach (var prop in props)
                            Log.WriteLine(1, $"[RMC RichPresence] {prop}", Color.Blue, client);
                        serializedProps = new PresencePropertyListSerializer(props).Serialize();
                        elem = new PresenceElement()
                        {
                            Pid = pid,
                            OverrideStatus = true,
                            UnkInt2 = 0x69,
                            PropsBuffer = new QBuffer(serializedProps)
                        };
                        presenceElements.Add(elem);
                    }
                    reply = new RMCPacketResponseRichPresenceService_GetPresence(presenceElements);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC RichPresence] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
