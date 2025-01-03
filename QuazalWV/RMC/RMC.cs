using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace QuazalWV
{
    public static class RMC
    {
        public const uint MaxRmcPayloadSize = 963;
        public static void HandlePacket(UdpClient udp, QPacket p)
        {
            ClientInfo client = Global.GetClientByIDrecv(p.m_uiSignature);
            if (client == null)
                return;
            client.sessionID = p.m_bySessionID;
            if (p.uiSeqId > client.seqCounter)
                client.seqCounter = p.uiSeqId;
            client.udp = udp;
            if (p.flags.Contains(QPacket.PACKETFLAG.FLAG_ACK))
                return;
            WriteLog(10, "Handling packet...");
            RMCP rmc = new RMCP(p);
            if (rmc.isRequest)
                HandleRequest(client, p, rmc);
            else
                HandleResponse(client, p, rmc);
        }

        public static void HandleResponse(ClientInfo client, QPacket p, RMCP rmc)
        {
            ProcessResponse(client, p, rmc);
            WriteLog(1, "Received Response : " + rmc.ToString());
        }

        public static void ProcessResponse(ClientInfo client, QPacket p, RMCP rmc)
        {
            MemoryStream m = new MemoryStream(p.payload);
            m.Seek(rmc._afterProtocolOffset, 0);
            rmc.success = m.ReadByte() == 1;
            if (rmc.success)
            {
                rmc.callID = Helper.ReadU32(m);
                rmc.methodID = Helper.ReadU32(m);
            }
            else
            {
                rmc.error = Helper.ReadU32(m);
                rmc.callID = Helper.ReadU32(m);
            }
            WriteLog(1, "Got response for Protocol " + rmc.proto + " = " + (rmc.success ? "Success" : "Fail"));
            if (p.flags.Contains(QPacket.PACKETFLAG.FLAG_NEED_ACK))
                SendACK(client.udp, p, client);
        }

        public static void HandleRequest(ClientInfo client, QPacket p, RMCP rmc)
        {
            ProcessRequest(client, p, rmc);
            if (rmc.callID > client.callCounterRMC)
                client.callCounterRMC = rmc.callID;
            WriteLog(1, "Received Request : " + rmc.ToString());
            string payload = rmc.PayLoadToString();
            if (payload != "")
                WriteLog(5, payload);
            switch (rmc.proto)
            {
                case RMCP.PROTOCOL.NATTraversalRelayService:
                    NATTraversalService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.AuthenticationService:
                    AuthenticationService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.SecureService:
                    SecureService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.FriendsService:
                    FriendsService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.MatchMakingService:
                    MatchMakingService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.MessagingService:
                    MessagingService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.PersistentStoreService:
                    PersistentStoreService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.MessageDeliveryService:
                    MessageDeliveryService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.UbiAccountMgmtService:
                    UbiAccountMgmtService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.NewsService:
                    NewsService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.UbiNewsService:
                    UbiNewsService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.PrivilegesService:
                    PrivilegesService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.TrackingService:
                    TrackingService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.LocalizationService:
                    LocalizationService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.GameSessionService:
                    GameSessionService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.HermesPlayerStatsService:
                    HermesPlayerStatisticsService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.RichPresenceService:
                    RichPresenceService.HandleRequest(p, rmc, client);
					break;
				case RMCP.PROTOCOL.TrackingExtService:
                    TrackingExtService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.GameInfoService:
                    GameInfoService.HandleRequest(p, rmc, client);
                    break;
                case RMCP.PROTOCOL.UplayWinService:
                    UplayWinService.HandleRequest(p, rmc, client);
                    break;
                default:
                    WriteLog(1, $"Error: No handler implemented for packet protocol {rmc.proto}");
                    break;
            }
        }

        public static void ProcessRequest(ClientInfo client, QPacket p, RMCP rmc)
        {
            MemoryStream m = new MemoryStream(p.payload);
            m.Seek(rmc._afterProtocolOffset, 0);
            rmc.callID = Helper.ReadU32(m);
            rmc.methodID = Helper.ReadU32(m);
            switch (rmc.proto)
            {
                case RMCP.PROTOCOL.AuthenticationService:
                    AuthenticationService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.SecureService:
                    SecureService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.FriendsService:
                    FriendsService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.MessagingService:
                    MessagingService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.PersistentStoreService:
                    PersistentStoreService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.MessageDeliveryService:
                    MessageDeliveryService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.UbiAccountMgmtService:
                    UbiAccountMgmtService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.NewsService:
                    NewsService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.UbiNewsService:
                    UbiNewsService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.PrivilegesService:
                    PrivilegesService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.TrackingService:
                    TrackingService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.LocalizationService:
                    LocalizationService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.GameSessionService:
                    GameSessionService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.HermesPlayerStatsService:
                    HermesPlayerStatisticsService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.RichPresenceService:
                    RichPresenceService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.TrackingExtService:
                    TrackingExtService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.GameInfoService:
                    GameInfoService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.UplayWinService:
                    UplayWinService.ProcessRequest(m, rmc);
                    break;
                case RMCP.PROTOCOL.MatchMakingService:
                case RMCP.PROTOCOL.NATTraversalRelayService:
                    break;
                default:
                    WriteLog(1, "Error: No request reader implemented for packet protocol " + rmc.proto);
                    break;
            }
        }


        public static void SendResponseWithACK(UdpClient udp, QPacket p, RMCP rmc, ClientInfo client, RMCPResponse reply, bool useCompression = true, uint error = 0)
        {
            WriteLog(2, "Response : " + reply.ToString());
            string payload = reply.PayloadToString();
            if (payload != "")
                WriteLog(5, "Response Data Content : \n" + payload);
            SendACK(udp, p, client);
            SendResponsePacket(udp, p, rmc, client, reply, useCompression, error);
        }

        private static void SendACK(UdpClient udp, QPacket p, ClientInfo client)
        {
            QPacket np = new QPacket(p.ToBuffer());
            np.flags = new List<QPacket.PACKETFLAG>() { QPacket.PACKETFLAG.FLAG_ACK };
            np.m_oSourceVPort = p.m_oDestinationVPort;
            np.m_oDestinationVPort = p.m_oSourceVPort;
            np.m_uiSignature = client.IDsend;
            np.payload = new byte[0];
            np.payloadSize = 0;
            WriteLog(10, "send ACK packet");
            Send(udp, np, client);
        }

        private static void SendResponsePacket(UdpClient udp, QPacket p, RMCP rmc, ClientInfo client, RMCPResponse reply, bool useCompression, uint error)
        {
            MemoryStream m = new MemoryStream();
            if ((ushort)rmc.proto < 0x7F)
            {
                Helper.WriteU8(m, (byte)rmc.proto);
            }
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
            QPacket np = new QPacket(p.ToBuffer())
            {
                flags = new List<QPacket.PACKETFLAG>() { QPacket.PACKETFLAG.FLAG_NEED_ACK },
                m_oSourceVPort = p.m_oDestinationVPort,
                m_oDestinationVPort = p.m_oSourceVPort,
                m_uiSignature = client.IDsend
            };
            MakeAndSend(client, np, m.ToArray());
        }
        
        public static void SendRequestPacket(UdpClient udp, QPacket p, RMCP rmc, ClientInfo client, RMCPResponse packet, bool useCompression, uint error)
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
            QPacket np = new QPacket(p.ToBuffer());
            np.flags = new List<QPacket.PACKETFLAG>() { QPacket.PACKETFLAG.FLAG_NEED_ACK };
            np.m_uiSignature = client.IDsend;
            MakeAndSend(client, np, m.ToArray());
        }

        public static void MakeAndSend(ClientInfo client, QPacket np, byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            if (data.Length < MaxRmcPayloadSize)
            {
                np.uiSeqId++;
                np.payload = data;
                np.payloadSize = (ushort)np.payload.Length;
                WriteLog(10, "sent packet");
                Send(client.udp, np, client);
            }
            else
            {
                np.flags.Add(QPacket.PACKETFLAG.FLAG_RELIABLE);
                int pos = 0;
                m.Seek(0, 0);
                np.m_byPartNumber = 0;
                while (pos < data.Length)
                {
                    np.uiSeqId = client.seqCounterReliable++;
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
                }
                WriteLog(10, "sent packets");
            }
        }

        public static void Send(UdpClient udp, QPacket p, ClientInfo client)
        {
            byte[] data = p.ToBuffer();
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append(b.ToString("X2") + " ");
            WriteLog(5, "send : " + p.ToStringShort());
            WriteLog(10, "send : " + sb.ToString());
            WriteLog(10, "send : " + p.ToStringDetailed());
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
                                         + paramStr + "\"]");
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
            QPacket q = new QPacket();
            q.m_oSourceVPort = new QPacket.VPort(0x31);
            q.m_oDestinationVPort = new QPacket.VPort(0x3f);
            q.type = QPacket.PACKETTYPE.DATA;
            q.flags = new List<QPacket.PACKETFLAG>();
            q.payload = new byte[0];
            q.uiSeqId = (ushort)(++client.seqCounter);
            q.m_bySessionID = client.sessionID;
            RMCP rmc = new RMCP();
            rmc.proto = RMCP.PROTOCOL.GlobalNotificationEventService;
            rmc.methodID = 1;
            rmc.callID = ++client.callCounterRMC;
            RMCPCustom reply = new RMCPCustom();
            reply.buffer = payload;
            RMC.SendRequestPacket(client.udp, q, rmc, client, reply, true, 0);
        }

        private static void WriteLog(int priority, string s)
        {
            Log.WriteLine(priority, "[RMC] " + s);
        }

    }
}
