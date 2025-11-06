using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace QuazalWV
{
    public static class RMC
    {
        public const uint MaxRmcPayloadSize = 963;
        public static void HandlePacket(UdpClient udp, PrudpPacket p)
        {
            ClientInfo client = Global.GetClientByIDrecv(p.m_uiSignature);
            if (client == null)
                return;
            client.sessionID = p.m_bySessionID;
            if (p.uiSeqId > client.gameSeqId)
                client.gameSeqId = p.uiSeqId;
            client.udp = udp;
            if (p.flags.Contains(PrudpPacket.PACKETFLAG.FLAG_ACK))
                return;
            WriteLog(10, "Handling packet...", client);
            RMCP rmc = new RMCP(p);
            if (rmc.isRequest)
                HandleRequest(client, p, rmc);
            else
                HandleResponse(client, p, rmc);
        }

        public static void HandleResponse(ClientInfo client, PrudpPacket p, RMCP rmc)
        {
            ProcessResponse(client, p, rmc);
            WriteLog(1, $"Received Response : {rmc}", client);
        }

        public static void ProcessResponse(ClientInfo client, PrudpPacket p, RMCP rmc)
        {
            MemoryStream m = new MemoryStream(p.payload);
            m.Seek(rmc._afterProtocolOffset, 0);
            rmc.success = m.ReadByte() == 1;
            if (rmc.success)
            {
                rmc.callID = Helper.ReadU32(m);
                rmc.methodID = Helper.ReadU32(m) - 0x8000;
            }
            else
            {
                rmc.error = Helper.ReadU32(m);
                rmc.callID = Helper.ReadU32(m);
            }
            if (p.flags.Contains(PrudpPacket.PACKETFLAG.FLAG_NEED_ACK))
                SendACK(client.udp, p, client);
        }

        public static void HandleRequest(ClientInfo client, PrudpPacket p, RMCP rmc)
        {
            if (client.User == null && rmc.proto != RMCP.PROTOCOL.Authentication)
            {
                Log.WriteLine(1, $"Dropped Request from NULL client : [RMC Packet : Proto = {rmc.proto} CallID={rmc.callID} MethodID={rmc.methodID}]", LogSource.RMC, Color.Red, client);
                SendACK(client.udp, p, client); // Send ACK, to prevent connection issues
                return;
            }
            ProcessRequest(client, p, rmc);
            if (rmc.callID > client.callCounterRMC)
                client.callCounterRMC = rmc.callID;
            WriteLog(1, $"Received Request : {rmc}", client);
            string payload = rmc.PayLoadToString();
            if (payload != "")
                WriteLog(5, payload, client);
            switch (rmc.proto)
            {
                case RMCP.PROTOCOL.NatTraversalRelay:
                    NATTraversalService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.Authentication:
                    AuthenticationService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.Secure:
                    SecureService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.Friends:
                    FriendsService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.Messaging:
                    MessagingService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.PersistentStore:
                    PersistentStoreService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.MessageDelivery:
                    MessageDeliveryService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.UbiAccountMgmt:
                    UbiAccountMgmtService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.News:
                    NewsService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.UbiNews:
                    UbiNewsService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.Privileges:
                    PrivilegesService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.Tracking:
                    TrackingService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.Localization:
                    LocalizationService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.GameSession:
                    GameSessionService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.HermesPlayerStats:
                    HermesPlayerStatisticsService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.RichPresence:
                    RichPresenceService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.TrackingExt:
                    TrackingExtService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.GameInfo:
                    GameInfoService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.ContactsExtensions:
                    ContactsService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.UplayWin:
                    UplayWinService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.Virgin:
                    VirginService.HandleRequest(p, rmc, client);
                    break;
                default:
                    WriteLog(1, $"Error: No handler implemented for packet protocol {rmc.proto}", client);
                    break;
            }
        }

        public static void ProcessRequest(ClientInfo client, PrudpPacket p, RMCP rmc)
        {
            MemoryStream m = new MemoryStream(p.payload);
            m.Seek(rmc._afterProtocolOffset, 0);
            rmc.callID = Helper.ReadU32(m);
            rmc.methodID = Helper.ReadU32(m);
            switch (rmc.proto)
            {
                case RMCP.PROTOCOL.NatTraversalRelay:
                    NATTraversalService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.Authentication:
                    AuthenticationService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.Secure:
                    SecureService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.Friends:
                    FriendsService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.Messaging:
                    MessagingService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.PersistentStore:
                    PersistentStoreService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.MessageDelivery:
                    MessageDeliveryService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.UbiAccountMgmt:
                    UbiAccountMgmtService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.News:
                    NewsService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.UbiNews:
                    UbiNewsService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.Privileges:
                    PrivilegesService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.Tracking:
                    TrackingService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.Localization:
                    LocalizationService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.GameSession:
                    GameSessionService.ProcessRequest(m, rmc, client);
                    break;
                case RMCP.PROTOCOL.HermesPlayerStats:
                    HermesPlayerStatisticsService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.RichPresence:
                    RichPresenceService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.TrackingExt:
                    TrackingExtService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.GameInfo:
                    GameInfoService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.ContactsExtensions:
                    ContactsService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.Virgin:
                    VirginService.ProcessRequest(m, rmc, client);
                    break;
                case RMCP.PROTOCOL.UplayWin:
                    UplayWinService.ProcessRequest(m, rmc);
                    break;
                default:
                    WriteLog(1, $"Error: No request reader implemented for packet protocol {rmc.proto}", client);
                    break;
            }
        }


        public static void SendResponseWithACK(UdpClient udp, PrudpPacket p, RMCP rmc, ClientInfo client, RMCPResponse reply, bool useCompression = true, uint error = 0)
        {
            WriteLog(2, $"Response : {reply}", client);
            string payload = reply.PayloadToString();
            if (payload != "")
                WriteLog(5, $"Response Data Content : \n{payload}", client);
            SendACK(udp, p, client);
            SendResponsePacket(udp, p, rmc, client, reply, useCompression, error);
        }

        private static void SendACK(UdpClient udp, PrudpPacket p, ClientInfo client)
        {
            PrudpPacket np = new PrudpPacket(p.ToBuffer())
            {
                flags = new List<PrudpPacket.PACKETFLAG>() { PrudpPacket.PACKETFLAG.FLAG_ACK },
                m_oSourceVPort = p.m_oDestinationVPort,
                m_oDestinationVPort = p.m_oSourceVPort,
                m_uiSignature = p.m_oSourceVPort.port == 15 ? client.playerSignature : client.trackingSignature,
                payload = new byte[0],
                payloadSize = 0
            };
            WriteLog(10, "send ACK packet", client);
            Send(udp, np, client);
        }

        private static void SendResponsePacket(UdpClient udp, PrudpPacket p, RMCP rmc, ClientInfo client, RMCPResponse reply, bool useCompression, uint error)
        {
            MemoryStream m = new MemoryStream();
            if ((ushort)rmc.proto < 0x7F)
                Helper.WriteU8(m, (byte)rmc.proto);
            else
            {
                Helper.WriteU8(m, 0x7F);
                Helper.WriteU16(m, (ushort)rmc.proto);
            }
            byte[] buff;
            if (error == 0)
            {
                Helper.WriteU8(m, 0x1);
                Helper.WriteU32(m, rmc.callID);
                Helper.WriteU32(m, rmc.methodID | 0x8000);
                buff = reply.ToBuffer();
                if(buff != null) m.Write(buff, 0, buff.Length);                
            }
            else
            {
                Helper.WriteU8(m, 0);
                Helper.WriteU32(m, error);
                Helper.WriteU32(m, rmc.callID);
            } 
            buff = m.ToArray();
            m = new MemoryStream();
            Helper.WriteU32(m, (uint)buff.Length);
            m.Write(buff, 0, buff.Length);
            PrudpPacket np = new PrudpPacket(p.ToBuffer())
            {
                flags = new List<PrudpPacket.PACKETFLAG>() { PrudpPacket.PACKETFLAG.FLAG_NEED_ACK },
                m_oSourceVPort = p.m_oDestinationVPort,
                m_oDestinationVPort = p.m_oSourceVPort,
                m_uiSignature = p.m_oSourceVPort.port == 15 ? client.playerSignature : client.trackingSignature
            };
            MakeAndSend(client, np, m.ToArray());
        }
        
        public static void SendRequestPacket(PrudpPacket p, RMCP rmc, ClientInfo client, RMCPResponse packet, bool useCompression, uint error)
        {
            MemoryStream m = new MemoryStream();
            if ((ushort)rmc.proto < 0x7F)
                Helper.WriteU8(m, (byte)((byte)rmc.proto | 0x80));
            else
            {
                Helper.WriteU8(m, 0xFF);
                Helper.WriteU16(m, (ushort)rmc.proto);
            }
            byte[] buff;
            if (error == 0)
            {
                Helper.WriteU32(m, rmc.callID);
                Helper.WriteU32(m, rmc.methodID);
                buff = packet.ToBuffer();
                m.Write(buff, 0, buff.Length);
            }
            else
            {
                Helper.WriteU32(m, error);
                Helper.WriteU32(m, rmc.callID);
            }
            buff = m.ToArray();
            m = new MemoryStream();
            Helper.WriteU32(m, (uint)buff.Length);
            m.Write(buff, 0, buff.Length);
            PrudpPacket np = new PrudpPacket(p.ToBuffer())
            {
                flags = new List<PrudpPacket.PACKETFLAG>() { PrudpPacket.PACKETFLAG.FLAG_NEED_ACK },
                m_uiSignature = client.IDsend
            };
            MakeAndSend(client, np, m.ToArray());
        }

        public static void MakeAndSend(ClientInfo client, PrudpPacket np, byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            if (data.Length < MaxRmcPayloadSize)
            {
                var seqId = np.m_oDestinationVPort.port == 15 ? client.seqIdUnreliable++ : client.seqIdUnreliableTracking++;
                np.uiSeqId = seqId;
                //np.uiSeqId++;
                np.payload = data;
                np.payloadSize = (ushort)np.payload.Length;
                WriteLog(10, "sent packet", client);
                Send(client.udp, np, client);
            }
            else
            {
                np.flags.Add(PrudpPacket.PACKETFLAG.FLAG_RELIABLE);
                int pos = 0;
                m.Seek(0, 0);
                np.m_byPartNumber = 0;
                while (pos < data.Length)
                {
                    // response fragmentation for Tracking is unexpected
                    np.uiSeqId = client.seqIdReliable++;
                    bool isLast = false;
                    int len = (int)MaxRmcPayloadSize;
                    if (len + pos >= data.Length)
                    {
                        len = data.Length - pos;
                        isLast = true;
                    }
                    if (!isLast)
                        np.m_byPartNumber++;
                    else
                        np.m_byPartNumber = 0;
                    byte[] buff = new byte[len];
                    m.Read(buff, 0, len);
                    np.payload = buff;
                    np.payloadSize = (ushort)np.payload.Length;
                    Send(client.udp, np, client);
                    pos += len;
                    Thread.Sleep(1);
                }
                WriteLog(10, "sent packets", client);
            }
        }

        public static void Send(UdpClient udp, PrudpPacket p, ClientInfo client)
        {
            byte[] data = p.ToBuffer();
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append(b.ToString("X2") + " ");
            WriteLog(5, "send : " + p.ToStringShort(), client);
            WriteLog(10, "send : " + sb.ToString(), client);
            WriteLog(10, "send : " + p.ToStringDetailed(), client);
            udp.Send(data, data.Length, client.ep);
            Log.LogPacket(true, data);
        }

        public static void SendNotification(ClientInfo client, uint source, uint type, uint subType, uint param1, uint param2, uint param3, string paramStr)
        {
            WriteLog(1, "Send Notification: [" + source.ToString("X8") + " " 
                                         + type.ToString("X8") + " "
                                         + subType.ToString("X8") + " " 
                                         + param1.ToString("X8") + " "
                                         + param2.ToString("X8") + " "
                                         + param3.ToString("X8") + " \""
                                         + paramStr + "\"]", client);
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, source);
            Helper.WriteU32(m, type * 1000 + subType);
            Helper.WriteU32(m, param1);
            Helper.WriteU32(m, param2);
            Helper.WriteU16(m, (ushort)(paramStr.Length + 1));
            foreach (char c in paramStr)
                m.WriteByte((byte)c);
            m.WriteByte(0);
            Helper.WriteU32(m, param3);
            byte[] payload = m.ToArray();
            PrudpPacket q = new PrudpPacket
            {
                m_oSourceVPort = new PrudpPacket.VPort(0x31),
                m_oDestinationVPort = new PrudpPacket.VPort(0x3f),
                type = PrudpPacket.PACKETTYPE.DATA,
                flags = new List<PrudpPacket.PACKETFLAG>(),
                payload = new byte[0],
                uiSeqId = ++client.gameSeqId,
                m_bySessionID = client.sessionID
            };
            RMCP rmc = new RMCP
            {
                proto = RMCP.PROTOCOL.GlobalNotificationEvent,
                methodID = 1,
                callID = ++client.callCounterRMC
            };
            RMCPCustom reply = new RMCPCustom
            {
                buffer = payload
            };
            SendRequestPacket(q, rmc, client, reply, true, 0);
        }

        public static void SendRequest(ClientInfo client, RMCPRequest req, RMCP.PROTOCOL protocol, uint methodId)
        {
            byte[] payload = req.ToBuffer();
            PrudpPacket q = new PrudpPacket
            {
                m_oSourceVPort = new PrudpPacket.VPort(0x31),
                m_oDestinationVPort = new PrudpPacket.VPort(0x3f),
                type = PrudpPacket.PACKETTYPE.DATA,
                flags = new List<PrudpPacket.PACKETFLAG>(),
                payload = new byte[0],
                uiSeqId = ++client.gameSeqId,
                m_bySessionID = client.sessionID
            };
            RMCP rmc = new RMCP
            {
                proto = protocol,
                methodID = methodId,
                callID = ++client.callCounterRMC
            };
            RMCPCustom reply = new RMCPCustom
            {
                buffer = payload
            };
            SendRequestPacket(q, rmc, client, reply, true, 0);
        }

        private static void WriteLog(int priority, string content, ClientInfo client)
        {
            Log.WriteLine(priority, content, LogSource.RMC, null, client);
        }
    }
}
