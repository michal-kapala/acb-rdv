using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuazalWV
{
    public static class RichPresenceService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.RichPresence;

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
                    var reqSetPresence = (RMCPacketRequestRichPresenceService_SetPresence)rmc.request;
                    var props = reqSetPresence.Props;
                    client.PresenceProps = props;
                    if (props != null && props.Count > 0)
                        Log.WriteRmcLine(2, string.Join("\n", props), protocol, LogSource.RMC, Color.Blue, client);
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 2:
                    // Run presence fetching asynchronously to avoid blocking
                    Task.Run(() =>
                    {
                        try
                        {
                            var reqGetPresence = (RMCPacketRequestRichPresenceService_GetPresence)rmc.request;
                            var presenceElements = new List<PresenceElement>(reqGetPresence.Pids.Count);
                            List<ClientInfo> clientsSnapshot;
                            lock (Global.Clients)
                                clientsSnapshot = Global.Clients.ToList();
                            var clientDict = new Dictionary<uint, ClientInfo>();
                            foreach (var c in clientsSnapshot)
                            {
                                if (c?.User == null)
                                    continue;

                                if (!clientDict.ContainsKey(c.User.Pid))
                                    clientDict[c.User.Pid] = c;
                            }
                            var elementsLock = new object();
                            Parallel.ForEach(reqGetPresence.Pids, pid =>
                            {
                                try
                                {
                                    ClientInfo friend;
                                    if (clientDict.TryGetValue(pid, out friend) &&
                                        friend.PresenceProps != null &&
                                        friend.PresenceProps.Count > 0)
                                    {
                                        // Log once to achieve minimal I/O for better performance
                                        var fProps = friend.PresenceProps;
                                        Log.WriteRmcLine(1, string.Join("\n", fProps), protocol, LogSource.RMC, Color.Blue, client);
                                        var serializedProps = new PresencePropertyListSerializer(fProps).Serialize();
                                        var element = new PresenceElement
                                        {
                                            Pid = pid,
                                            OverrideStatus = true,
                                            UnkInt2 = 0x69,
                                            PropsBuffer = new QBuffer(serializedProps)
                                        };

                                        lock (elementsLock)
                                            presenceElements.Add(element);
                                    }
                                    else
                                    {
                                        var emptyElement = new PresenceElement
                                        {
                                            Pid = pid,
                                            OverrideStatus = true,
                                            UnkInt2 = 0x69,
                                            PropsBuffer = new QBuffer(new byte[0])
                                        };
                                        lock (elementsLock)
                                            presenceElements.Add(emptyElement);
                                    }
                                }
                                catch (System.Exception ex)
                                {
                                    Log.WriteLine(1, $"[RichPresence] Error processing PID {pid}: {ex.Message}", LogSource.RMC, Color.Red, client);
                                }
                            });
                            var replyLocal = new RMCPacketResponseRichPresenceService_GetPresence(presenceElements);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, replyLocal);
                        }
                        catch (System.Exception ex)
                        {
                            Log.WriteLine(1, "[RichPresence] Error in async presence handling: " + ex.Message, LogSource.RMC, Color.Red, client);
                        }
                    });
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }
    }
}
