using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Collections.Concurrent;
using System.Linq;

namespace QuazalWV
{
    
    public class ExpiringLockManager<TKey>
    {
        private class LockWrapper
        {
            public object LockObject { get; } = new object();
            public DateTime LastUsed { get; set; } = DateTime.UtcNow;
        }

        private readonly ConcurrentDictionary<TKey, LockWrapper> _locks = new ConcurrentDictionary<TKey, LockWrapper>();
        private readonly TimeSpan _expiration;
        private readonly Timer _cleanupTimer;

        public ExpiringLockManager(TimeSpan expiration, TimeSpan cleanupInterval)
        {
            _expiration = expiration;
            _cleanupTimer = new Timer(Cleanup, null, cleanupInterval, cleanupInterval);
        }

        public object GetLock(TKey key)
        {
            var wrapper = _locks.GetOrAdd(key, _ => new LockWrapper());
            wrapper.LastUsed = DateTime.UtcNow;
            return wrapper.LockObject;
        }

        private void Cleanup(object state)
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _locks)
            {
                if (now - kvp.Value.LastUsed > _expiration)
                {
                    Log.WriteLine(1, "removed lock which was no longer used", Color.Red);
                    _locks.TryRemove(kvp.Key, out _);
                }
            }
        }

        public void Dispose()
        {
            _cleanupTimer.Dispose();
        }
    }
    public static class QPacketHandler
    {
        public static List<ulong> timeToIgnore = new List<ulong>();
        public static Random rand = new Random();
        private static ExpiringLockManager<IPAddress> lockManager = new ExpiringLockManager<IPAddress>(  expiration: TimeSpan.FromMinutes(5), cleanupInterval: TimeSpan.FromMinutes(1) );

        public static QPacket ProcessSYN(QPacket p, IPEndPoint ep, out ClientInfo client)
        {
            client = Global.GetClientByEndPoint(ep);
            if (client == null)
            {
                //Log.WriteLine(2, "[QUAZAL] Creating new client data...");
                client = new ClientInfo
                {
                    ep = ep,
                    IDrecv = Global.idCounter++,
                    PID = Global.pidCounter++
                };
                Global.Clients.Add(client);
            }
            QPacket reply = new QPacket
            {
                m_oSourceVPort = p.m_oDestinationVPort,
                m_oDestinationVPort = p.m_oSourceVPort,
                flags = new List<QPacket.PACKETFLAG>() { QPacket.PACKETFLAG.FLAG_ACK },
                type = QPacket.PACKETTYPE.SYN,
                m_bySessionID = p.m_bySessionID,
                m_uiSignature = p.m_uiSignature,
                uiSeqId = p.uiSeqId,
                m_uiConnectionSignature = client.IDrecv,
                payload = new byte[0]
            };
            // for localhost testing, remove from prod
            if (p.m_oSourceVPort.port == 15)
                Thread.Sleep(50);

            return reply;
        }

        public static QPacket ProcessCONNECT(ClientInfo client, QPacket p)
        {

            {
                client.IDsend = p.m_uiConnectionSignature;
                if (p.m_oSourceVPort.port == 15)
                    client.playerSignature = p.m_uiConnectionSignature;
                else
                    client.trackingSignature = p.m_uiConnectionSignature;
                QPacket reply = new QPacket
                {
                    m_oSourceVPort = p.m_oDestinationVPort,
                    m_oDestinationVPort = p.m_oSourceVPort,
                    flags = new List<QPacket.PACKETFLAG>() { QPacket.PACKETFLAG.FLAG_ACK },
                    type = QPacket.PACKETTYPE.CONNECT,
                    m_bySessionID = p.m_bySessionID,
                    m_uiSignature = p.m_uiConnectionSignature,
                    uiSeqId = p.uiSeqId,
                    m_uiConnectionSignature = client.IDrecv
                };
                // for localhost testing, remove from prod
                if (p.m_oSourceVPort.port == 15)
                    Thread.Sleep(50);

                if (p.payload != null && p.payload.Length > 0)
                    reply.payload = MakeConnectPayload(client, p);
                else
                    reply.payload = new byte[0];
                return reply;
            }
        }

        private static byte[] MakeConnectPayload(ClientInfo client, QPacket p)
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
            uint pid = Helper.ReadU32(m); //user pid
            uint cid = Helper.ReadU32(m); //connection id
            uint responseCode = Helper.ReadU32(m);

            // Tracking user connection
            if (p.m_oSourceVPort.port == 0xE)
            {
                // response code dumped from the original traffic
                responseCode = 0xF94C56FB;
                //Log.WriteLine(1, $"[UDP Secure] CONNECT for Tracking user, response code 0x{responseCode:X8}");
                m = new MemoryStream();
                Helper.WriteU32(m, 4);
                Helper.WriteU32(m, responseCode);
                return m.ToArray();
            }
            //Log.WriteLine(1, $"[UDP Secure] CONNECT: PID: 0x{pid:X8}, CID: {cid}, response code 0x{responseCode:X8}");
            m = new MemoryStream();
            Helper.WriteU32(m, 4);
            Helper.WriteU32(m, responseCode + 1);
            return m.ToArray();
        }

        public static QPacket ProcessDISCONNECT(ClientInfo client, QPacket p)
        {
            QPacket reply = new QPacket
            {
                m_oSourceVPort = p.m_oDestinationVPort,
                m_oDestinationVPort = p.m_oSourceVPort,
                flags = new List<QPacket.PACKETFLAG>() { QPacket.PACKETFLAG.FLAG_ACK },
                type = QPacket.PACKETTYPE.DISCONNECT,
                m_bySessionID = p.m_bySessionID,
                m_uiSignature = p.m_oSourceVPort.port == 15 ? client.playerSignature - 0x10000 : client.trackingSignature - 0x10000,
                uiSeqId = p.uiSeqId,
                payload = new byte[0]
            };
            return reply;
        }

        public static QPacket ProcessPING(ClientInfo client, QPacket p)
        {
            QPacket reply = new QPacket
            {
                m_oSourceVPort = p.m_oDestinationVPort,
                m_oDestinationVPort = p.m_oSourceVPort,
                flags = new List<QPacket.PACKETFLAG>() { QPacket.PACKETFLAG.FLAG_ACK },
                type = QPacket.PACKETTYPE.PING,
                m_bySessionID = p.m_bySessionID,
                m_uiSignature = p.m_oSourceVPort.port == 15 ? client.playerSignature : client.trackingSignature,
                uiSeqId = p.uiSeqId,
                m_uiConnectionSignature = client.IDrecv,
                payload = new byte[0]
            };
            return reply;
        }

        public static void ProcessQpacket(QPacket p)
        {
            LogQpacket(p);
            p.packet_began_processing = true;
            //Log.WriteLine(1, $"[QUAZAL Packet Handler] Actual Processing{p.uiSeqId} connid {p.m_bySessionID}");
            QPacket reply = null;
            ClientInfo client = null;
            if (p.type != QPacket.PACKETTYPE.SYN && p.type != QPacket.PACKETTYPE.NATPING)
                client = Global.GetClientByIDrecv(p.m_uiSignature);
            switch (p.type)
            {
                case QPacket.PACKETTYPE.SYN:
                    reply = ProcessSYN(p, p.ep, out client);
                    break;
                case QPacket.PACKETTYPE.CONNECT:
                    if (client != null && !p.flags.Contains(QPacket.PACKETFLAG.FLAG_ACK))
                    {
                        client.sPID = p.serverPID;
                        client.sPort = p.listenPort;
                        if (p.removeConnectPayload)
                        {
                            p.payload = new byte[0];
                            p.payloadSize = 0;
                        }
                        reply = ProcessCONNECT(client, p);
                    }
                    break;
                case QPacket.PACKETTYPE.DATA:
                    if (p.m_oSourceVPort.type == QPacket.STREAMTYPE.OldRVSec)
                        RMC.HandlePacket(p.listener, p);
                    break;
                case QPacket.PACKETTYPE.DISCONNECT:
                    if (client != null)
                        reply = ProcessDISCONNECT(client, p);
                    break;
                case QPacket.PACKETTYPE.PING:
                    if (client != null)
                        reply = ProcessPING(client, p);
                    break;
                case QPacket.PACKETTYPE.NATPING:
                    client = Global.GetClientByIDrecv(p.m_uiSignature);
                    ulong time = BitConverter.ToUInt64(p.payload, 5);
                    if (timeToIgnore.Contains(time))
                        timeToIgnore.Remove(time);
                    else
                    {
                        reply = p;
                        MemoryStream m = new MemoryStream();
                        byte b = (byte)(reply.payload[0] == 1 ? 0 : 1);
                        m.WriteByte(b);
                        uint rvcid = client.rvCID;
                        Helper.WriteU32(m, rvcid); //RVCID
                        Helper.WriteU64(m, time);
                        reply.payload = m.ToArray();
                        Send(p.source, reply, p.ep, p.listener);
                        m = new MemoryStream();
                        b = (byte)(b == 1 ? 0 : 1);
                        m.WriteByte(b);
                        Helper.WriteU32(m, rvcid); //RVCID
                        time = Helper.MakeTimestamp();
                        timeToIgnore.Add(time);
                        Helper.WriteU64(m, Helper.MakeTimestamp());
                        reply.payload = m.ToArray();
                    }
                    break;
            }
            if (reply != null)
                Send(p.source, reply, p.ep, p.listener);
            p.packet_processed = true;
            //Log.WriteLine(1, $"Finished Actual processing {p.uiSeqId} sessionid id {p.m_bySessionID}", Color.Blue);
        }

        public static void LogQpacket(QPacket p)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in p.data)
                sb.Append(b.ToString("X2") + " ");
            MemoryStream m = new MemoryStream(p.data);
            byte[] buff = new byte[(int)p.realSize];
            m.Seek(0, 0);
            m.Read(buff, 0, buff.Length);
            Log.LogPacket(false, buff);
            //Log.WriteLine(5, "[" + p.source + "] received : " + p.ToStringShort());
            //Log.WriteLine(10, "[" + p.source + "] received : " + sb.ToString());
            //Log.WriteLine(10, "[" + p.source + "] received : " + p.ToStringDetailed());
        }
        public static void ProcessPacket(string source, byte[] data, IPEndPoint ep, UdpClient listener, uint serverPID, ushort listenPort, bool removeConnectPayload = false)
        {
            //Log.WriteLine(1, $"processing packet {source} {serverPID}", Color.Brown);
            while (true)
            {
                QPacket p = new QPacket(data, source, ep, listener, serverPID, listenPort, removeConnectPayload);
                //Log.WriteLine(1, $"Logical Processing packet size {p.realSize} IP IS {p.ep.Address} sequence id {p.uiSeqId} connection id {p.m_bySessionID} sport {p.m_oSourceVPort} dport {p.m_oDestinationVPort}", Color.Red);
                //HandleQpacket(p);
                if (p.uiSeqId==0)
                {
                    try
                    {
                        ProcessQpacket(p);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, "[Processing Packet] Something went wrong: " + ex.Message, Color.Red);
                    }
                }
                else 
                {
                    var itemLock = lockManager.GetLock(p.ep.Address);
                    lock (itemLock)
                    {
                        try
                        {
                            ProcessQpacket(p);
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine(1, "[Processing Packet] Something went wrong: " + ex.Message,Color.Red);
                        }
                    }

                }
                //Log.WriteLine(1, $"Finished logical processing", Color.Red);
                if (p.realSize != data.Length)
                {
                    MemoryStream m = new MemoryStream(data);
                    int left = (int)(data.Length - p.realSize);
                    //Log.WriteLine(1, $"left bytes {left}");
                    byte[] newData = new byte[left];
                    m.Seek(p.realSize, 0);
                    m.Read(newData, 0, left);
                    data = newData;
                }
                else
                {
                    //Log.WriteLine(1, $"broke off packer {p.uiSeqId} sessionid id {p.m_bySessionID}", Color.Blue);
                    break;
                }
            }
        }

        public static void Send(string source, QPacket p, IPEndPoint ep, UdpClient listener)
        {

            byte[] data = p.ToBuffer();
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append(b.ToString("X2") + " ");
            //Log.WriteLine(5, "[" + source + "] send : " + p.ToStringShort());
            //Log.WriteLine(10, "[" + source + "] send : " + sb.ToString());
            //Log.WriteLine(10, "[" + source + "] send : " + p.ToStringDetailed());
            listener.Send(data, data.Length, ep);
            Log.LogPacket(true, data);
        }
    }
}
