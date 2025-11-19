using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Drawing;

namespace QuazalWV
{
    public static class PrudpHandler
    {
        public static List<ulong> timeToIgnore = new List<ulong>();
        public static Random rand = new Random();
        private static readonly ExpiringLockManager<IPAddress> lockManager = new ExpiringLockManager<IPAddress>(expiration: TimeSpan.FromSeconds(20), cleanupInterval: TimeSpan.FromSeconds(10));

        public static PrudpPacket ProcessSYN(PrudpPacket p, IPEndPoint ep, out ClientInfo client)
        {
            client = Global.GetClientByEndPoint(ep);
            if (client == null)
            {
                client = new ClientInfo
                {
                    ep = ep,
                    IDrecv = Global.IdCounter++,
                    PID = Global.PidCounter++
                };
                Global.Clients.Add(client);
            }
            PrudpPacket reply = new PrudpPacket
            {
                m_oSourceVPort = p.m_oDestinationVPort,
                m_oDestinationVPort = p.m_oSourceVPort,
                flags = new List<PrudpPacket.PACKETFLAG>() { PrudpPacket.PACKETFLAG.FLAG_ACK },
                type = PrudpPacket.PACKETTYPE.SYN,
                m_bySessionID = p.m_bySessionID,
                m_uiSignature = p.m_uiSignature,
                uiSeqId = p.uiSeqId,
                m_uiConnectionSignature = client.IDrecv,
                payload = new byte[0]
            };

            return reply;
        }

        public static PrudpPacket ProcessCONNECT(ClientInfo client, PrudpPacket p)
        {
            client.IDsend = p.m_uiConnectionSignature;
            if (p.m_oSourceVPort.port == 15)
                client.playerSignature = p.m_uiConnectionSignature;
            else
                client.trackingSignature = p.m_uiConnectionSignature;
            PrudpPacket reply = new PrudpPacket
            {
                m_oSourceVPort = p.m_oDestinationVPort,
                m_oDestinationVPort = p.m_oSourceVPort,
                flags = new List<PrudpPacket.PACKETFLAG>() { PrudpPacket.PACKETFLAG.FLAG_ACK },
                type = PrudpPacket.PACKETTYPE.CONNECT,
                m_bySessionID = p.m_bySessionID,
                m_uiSignature = p.m_uiConnectionSignature,
                uiSeqId = p.uiSeqId,
                m_uiConnectionSignature = client.IDrecv
            };

            if (p.payload != null && p.payload.Length > 0)
                reply.payload = MakeConnectPayload(client, p);
            else
                reply.payload = new byte[0];
            return reply;
        }

        private static byte[] MakeConnectPayload(ClientInfo client, PrudpPacket p)
        {
            MemoryStream m = new MemoryStream(p.payload);
            uint size = Helper.ReadU32(m);
            byte[] buff = new byte[size];
            m.Read(buff, 0, (int)size);
            size = Helper.ReadU32(m) - 16;
            buff = new byte[size];
            m.Read(buff, 0, (int)size);
            buff = Helper.Decrypt(client.sessionKey, buff);
            m = new MemoryStream(buff);
            uint pid = Helper.ReadU32(m);
            uint cid = Helper.ReadU32(m); // connection id
            uint challenge = Helper.ReadU32(m);

            // Tracking user connection
            if (p.m_oSourceVPort.port == 0xE)
            {
                // challenge dumped from the original traffic
                challenge = 0xF94C56FB;
                client.TrackingUser = DbHelper.GetUserByName("Tracking");
                Log.WriteLine(1, $"[Tracking] CONNECT for Tracking, challenge: 0x{challenge:X8}", LogSource.PRUDP, Color.Black, null, true);
                m = new MemoryStream();
                Helper.WriteU32(m, 4);
                Helper.WriteU32(m, challenge);
                return m.ToArray();
            }
            client.User = DbHelper.GetUserByID(pid);
            Log.WriteLine(1, $"CONNECT: PID: {pid}, CID: {cid}, challenge: 0x{challenge:X8}", LogSource.PRUDP, Color.Green);
            m = new MemoryStream();
            Helper.WriteU32(m, 4);
            Helper.WriteU32(m, challenge + 1);
            return m.ToArray();
        }

        public static PrudpPacket ProcessDISCONNECT(ClientInfo client, PrudpPacket p)
        {
            uint playerSig = client != null ? client.playerSignature - 0x10000 : 0;
            uint trackingSig = client != null ? client.trackingSignature - 0x10000 : 0;
            PrudpPacket reply = new PrudpPacket
            {
                m_oSourceVPort = p.m_oDestinationVPort,
                m_oDestinationVPort = p.m_oSourceVPort,
                flags = new List<PrudpPacket.PACKETFLAG>() { PrudpPacket.PACKETFLAG.FLAG_ACK },
                type = PrudpPacket.PACKETTYPE.DISCONNECT,
                m_bySessionID = p.m_bySessionID,
                m_uiSignature = p.m_oSourceVPort.port == 15 ? playerSig : trackingSig,
                uiSeqId = p.uiSeqId,
                payload = new byte[0]
            };
            return reply;
        }

        public static PrudpPacket ProcessPING(ClientInfo client, PrudpPacket p)
        {
            PrudpPacket reply = new PrudpPacket
            {
                m_oSourceVPort = p.m_oDestinationVPort,
                m_oDestinationVPort = p.m_oSourceVPort,
                flags = new List<PrudpPacket.PACKETFLAG>() { PrudpPacket.PACKETFLAG.FLAG_ACK },
                type = PrudpPacket.PACKETTYPE.PING,
                m_bySessionID = p.m_bySessionID,
                m_uiSignature = p.m_oSourceVPort.port == 15 ? client.playerSignature : client.trackingSignature,
                uiSeqId = p.uiSeqId,
                m_uiConnectionSignature = client.IDrecv,
                payload = new byte[0]
            };
            return reply;
        }

        public static void ProcessPacket(LogSource source, byte[] data, IPEndPoint ep, UdpClient listener, uint serverPID, ushort listenPort, bool removeConnectPayload = false)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append($"{b:X2} ");
            while (true)
            {
                PrudpPacket p = new PrudpPacket(data);
                MemoryStream m = new MemoryStream(data);
                byte[] buff = new byte[(int)p.realSize];
                m.Seek(0, 0);
                m.Read(buff, 0, buff.Length);
                Log.LogPacket(false, buff);
                Log.WriteLine(5, $"[{source}] received : {p.ToStringShort()}", LogSource.PRUDP, Color.Black, null, true);
                Log.WriteLine(10, $"[{source}] received : {sb}", LogSource.PRUDP, Color.Black, null, true);
                Log.WriteLine(10, $"[{source}] received : {p.ToStringDetailed()}", LogSource.PRUDP, Color.Black, null, true);
                PrudpPacket reply = null;
                ClientInfo client = null;
                if (p.type != PrudpPacket.PACKETTYPE.SYN && p.type != PrudpPacket.PACKETTYPE.NATPING)
                  client = Global.GetClientByIDrecv(p.m_uiSignature);
                var itemLock = lockManager.GetLock(ep.Address);
                lock (itemLock)
                {
                    switch (p.type)
                    {
                        case PrudpPacket.PACKETTYPE.SYN:
                            reply = ProcessSYN(p, ep, out client);
                            break;
                        case PrudpPacket.PACKETTYPE.CONNECT:
                            if (client != null && !p.flags.Contains(PrudpPacket.PACKETFLAG.FLAG_ACK))
                            {
                                client.sPID = serverPID;
                                client.sPort = listenPort;
                                if (removeConnectPayload)
                                {
                                    p.payload = new byte[0];
                                    p.payloadSize = 0;
                                }
                                reply = ProcessCONNECT(client, p);
                            }
                            break;
                        case PrudpPacket.PACKETTYPE.DATA:
                            if (p.m_oSourceVPort.type == PrudpPacket.STREAMTYPE.OldRVSec)
                                RMC.HandlePacket(listener, p);
                            break;
                        case PrudpPacket.PACKETTYPE.DISCONNECT:
                            reply = ProcessDISCONNECT(client, p);
                            // disconnection from RDV - notify friends
                            if (client != null && client.User != null && source == LogSource.RDV)
                            {
                                var rels = DbHelper.GetRelationships(client.User.Pid, (byte)PlayerRelationship.Friend);
                                uint friendPid;
                                ClientInfo friend;
                                foreach (var relationship in rels)
                                {
                                    friendPid = relationship.RequesterPid == client.User.Pid ? relationship.RequesteePid : relationship.RequesterPid;
                                    friend = Global.Clients.Find(c => c.User.Pid == friendPid);
                                    if (friend != null)
                                        NotificationManager.FriendStatusChanged(friend, client.User.Pid, client.User.Name, false);
                                }
                                Global.Clients.Remove(client);
                                Log.WriteLine(1, "DISCONNECT", LogSource.PRUDP, Color.Gray, client);
                            }
                            Send(source, reply, ep, listener);
                            break;
                        case PrudpPacket.PACKETTYPE.PING:
                            if (client != null)
                                reply = ProcessPING(client, p);
                            break;
                        case PrudpPacket.PACKETTYPE.NATPING:
                            client = Global.GetClientByIDrecv(p.m_uiSignature);
                            ulong time = BitConverter.ToUInt64(p.payload, 5);
                            if (timeToIgnore.Contains(time))
                                timeToIgnore.Remove(time);
                            else
                            {
                                reply = p;
                                m = new MemoryStream();
                                byte b = (byte)(reply.payload[0] == 1 ? 0 : 1);
                                m.WriteByte(b);
                                uint rvcid = client.rvCID;
                                Helper.WriteU32(m, rvcid); //RVCID
                                Helper.WriteU64(m, time);
                                reply.payload = m.ToArray();
                                Send(source, reply, ep, listener);
                                m = new MemoryStream();
                                b = (byte)(b == 1 ? 0 : 1);
                                m.WriteByte(b);
                                Helper.WriteU32(m, rvcid); //RVCID
                                ulong newTime = Helper.MakeTimestamp();
                                timeToIgnore.Add(newTime);
                                Helper.WriteU64(m, newTime);
                                reply.payload = m.ToArray();
                            }
                            break;
                    }
                    if (reply != null && p.type != PrudpPacket.PACKETTYPE.DISCONNECT)
                        Send(source, reply, ep, listener);
                }
                if (p.realSize != data.Length)
                {
                    m = new MemoryStream(data);
                    int left = (int)(data.Length - p.realSize);
                    byte[] newData = new byte[left];
                    m.Seek(p.realSize, 0);
                    m.Read(newData, 0, left);
                    data = newData;
                }
                else
                    break;
            }
        }

        public static void Send(LogSource source, PrudpPacket p, IPEndPoint ep, UdpClient listener)
        {
            byte[] data = p.ToBuffer();
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append($"{b:X2} ");
            Log.WriteLine(5, $"[{source}] send : {p.ToStringShort()}", LogSource.PRUDP, Color.Black, null, true);
            Log.WriteLine(10, $"[{source}] send : {sb}", LogSource.PRUDP, Color.Black, null, true);
            Log.WriteLine(10, $"[{source}] send : {p.ToStringDetailed()}", LogSource.PRUDP, Color.Black, null, true);
            listener.Send(data, data.Length, ep);
            Log.LogPacket(true, data);
        }
    }
}
