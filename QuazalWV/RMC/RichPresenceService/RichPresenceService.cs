using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

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

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    var reqSetPresence = (RMCPacketRequestRichPresenceService_SetPresence)rmc.request;
                    client.PresenceProps = reqSetPresence.Props;
                    foreach (var prop in reqSetPresence.Props)
                        Log.WriteLine(1, $"[RMC RichPresence] {prop}", Color.Blue, client);
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
                        var gameType = props.Find(prop => prop.Id == PresencePropertyId.GameType);
                        // missing game type prop
                        if (gameType == null)
                        {
                            var inSes = props.Find(prop => prop.Id == PresencePropertyId.IsInSession);
                            if (inSes == null)
                                Log.WriteLine(1, $"GetPresence: missing IsInSession property", Color.Red, client);
                            else if (inSes.Value == 10)
                            {
                                Log.WriteLine(1, $"GetPresence: missing GameType property", Color.Red, client);
                                props.Add(new PresenceProperty() { Id = PresencePropertyId.GameType, DataType = VariantType.Int32, Value = (uint)GameType.PUBLIC });
                            }
                        }
                        // expose private sessions for friends to join
                        else if ((GameType)gameType.Value == GameType.PRIVATE)
                            gameType.Value = (uint)GameType.PUBLIC;
                        serializedProps = new PresencePropertyListSerializer(props).Serialize();
                        Log.WriteLine(1, $"GetPresence for player {pid}:", Color.Blue, client);
                        Log.WriteLine(1, Encoding.ASCII.GetString(serializedProps), Color.Blue, client);
                        elem = new PresenceElement()
                        {
                            Pid = pid,
                            OverrideStatus = true,
                            UnkInt2 = 2,
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
