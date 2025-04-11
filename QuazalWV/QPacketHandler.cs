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
    public static class QPacketHandler
    {
        public static List<ulong> timeToIgnore = new List<ulong>();
        public static Random rand = new Random();
        private static ConcurrentDictionary<uint, List<QPacket>> sessionPackets = new ConcurrentDictionary<uint, List<QPacket>>();
        private static readonly object _lock = new object();


        //check if parameters were already processed
        public static bool IsCorrectPacket(QPacket pack, uint lastorder)
        {

            if (pack.uiSeqId == 1 && pack.packet_processed == false && pack.packet_began_processing == false)
            {
                //Log.WriteLine(1, $"[QUAZAL Packet Handler] Treating first packet  {pack.uiSeqId} connid {pack.m_bySessionID}");
                return true;
            }
            if (pack.uiSeqId == lastorder + 1)
            {
                //Log.WriteLine(1, $"[QUAZAL Packet Handler] Treating middle packet {pack.uiSeqId} connid {pack.m_bySessionID}");
                return true;
            }
            return false;

        }

        public static (QPacket, bool) FetchUnusedQpackets()
        {
            //Log.WriteLine(1, "[QUAZAL Packet Handler] Searching for untreated packets");
            lock (_lock)  // Ensure thread-safety
            {
                foreach (var ConPack in sessionPackets)
                {
                    List<QPacket> valueList = ConPack.Value;

                    if (valueList[0].packet_processed == false && IsCorrectPacket(valueList[0], 0))
                    {
                        return (valueList[0], true);
                    }
                    else
                    if (valueList[0].uiSeqId != 1)
                    {
                        //Log.WriteLine(5, $"[QUAZAL Packet Handler] Cannot treat packet {valueList[0].uiSeqId} connid {valueList[0].m_bySessionID}");
                        continue;
                    }

                    for (int it = 1; it < valueList.Count; it++)
                    {
                        QPacket pastVal = valueList[it - 1];

                        if (IsCorrectPacket(valueList[it], pastVal.uiSeqId))
                        {
                            return (valueList[it], true);
                        }
                        else if (valueList[it].uiSeqId != pastVal.uiSeqId + 1)
                        {
                            Log.WriteLine(1, $"[QUAZAL Packet Handler] Found out of order packet {valueList[0].uiSeqId} connid {valueList[0].m_bySessionID}");
                            break;
                        }
                    }
                }

            }
            return (new QPacket(), false);
        }
        //assumption 0 order does not matter
        // 1 is the first sequence and will reset the whole traffic
        // to treat old seqid after new seqid for same connection being reused
        public static void InsertPacket(QPacket packet, List<QPacket> packetlist)
        {
            for (int i = 0; i < packetlist.Count; i++)
            {
                if (packetlist[i].uiSeqId > packet.uiSeqId)
                {
                    //Log.WriteLine(1, $"[QUAZAL Packet Handler] insert {packet.uiSeqId} connid {packet.m_bySessionID}");
                    packetlist.Insert(i, packet);
                    return;

                }
            }
            //Log.WriteLine(1, $"[QUAZAL Packet Handler] This should not happen 4 {packet.uiSeqId} ");

        }
        public static void DumpPacketQueue(ConcurrentDictionary<uint, List<QPacket>> packetqueue)
        {

            foreach (var kvp in packetqueue)
            {
                //Log.WriteLine(1, $"Key: {kvp.Key}");
                //Log.WriteLine(1, "Values: " + string.Join(", ", kvp.Value.Select(p => p.uiSeqId)));
                //Log.WriteLine(1, ""); // For better formatting
            }
        }
        public static void HandleQpacket(QPacket packet)//(QPacket,bool)
        {
            DumpPacketQueue(sessionPackets);
            int i;
            if (packet.uiSeqId == 0 || packet.m_bySessionID == 0)
            {
                ProcessQpacket(packet);
                return;
            }
            lock (_lock)  // Ensure thread-safety
            {
                // If the key doesn't exist, create a new list for it
                if (!sessionPackets.ContainsKey(packet.m_bySessionID))
                {
                    sessionPackets[packet.m_bySessionID] = new List<QPacket>();
                    sessionPackets[packet.m_bySessionID].Add(packet);
                    DumpPacketQueue(sessionPackets);
                    if (packet.packet_processed == false && packet.packet_began_processing == false && packet.uiSeqId == 1)
                    {
                        //Log.WriteLine(1, $"[QUAZAL Packet Handler] processing x1 {packet.uiSeqId}");
                        ProcessQpacket(packet);
                        return;
                    }

                }
                else
                {
                    List<QPacket> sessionlist = sessionPackets[packet.m_bySessionID];
                    if (packet.uiSeqId == 1)
                    {
                        //Log.WriteLine(1, $"[QUAZAL Packet Handler] Resetting the connid {packet.m_bySessionID}");
                        for (i = 1; i < sessionlist.Count; i++)
                        {

                            if (sessionlist[i].packet_processed == false && sessionlist[i].packet_began_processing == false)
                            {
                                Log.WriteLine(1, $"[QUAZAL Packet Handler] Found out of order packet seqid {sessionlist[i].uiSeqId} connid {sessionlist[i].m_bySessionID}");
                                ProcessQpacket(sessionlist[i]);


                            }
                        }
                        //sessionlist.Clear();
                        //sessionlist.Add(packet);
                        //Log.WriteLine(1, $"[QUAZAL Packet Handler] processing the first packet for {packet.m_bySessionID}");
                        ProcessQpacket(packet);
                        return;
                    }
                    if (sessionlist[0].packet_processed == false && sessionlist[0].packet_began_processing == false)
                    {
                        if (sessionlist[0].uiSeqId == 1)
                        {
                            //Log.WriteLine(1, $"[QUAZAL Packet Handler] processing x1 {packet.uiSeqId}");
                            ProcessQpacket(packet);
                            return;
                        }
                        else
                        if (packet.uiSeqId == sessionlist[0].uiSeqId - 1)
                        {
                            //Log.WriteLine(1, $"[QUAZAL Packet Handler] waiting for first packet to do all of them {packet.m_bySessionID}");
                            sessionlist.Insert(0, packet);
                            return;
                        }
                    }
                    for (i = 1; i < sessionlist.Count; i++)
                    {
                        if (sessionlist[i].uiSeqId > packet.uiSeqId && sessionlist[i].uiSeqId != packet.uiSeqId + 1)
                        {
                            //Log.WriteLine(1, $"[QUAZAL Packet Handler] waiting for first packet to do all of them {packet.m_bySessionID}");
                            sessionlist.Insert(i, packet);
                            return;
                        }
                        if (sessionlist[i].uiSeqId == packet.uiSeqId)//remove if not good
                        {                                            //remove if not good
                            ProcessQpacket(packet);                  //remove if not good
                            return;                                  //remove if not good
                        }                                            //remove if not good
                        if (sessionlist[i].packet_processed == true && sessionlist[i].packet_began_processing == true)
                            continue;
                        if (sessionlist[i].uiSeqId > sessionlist[i - 1].uiSeqId + 1)
                        {
                            if (sessionlist[i].uiSeqId == packet.uiSeqId + 1)
                            {
                                //Log.WriteLine(1, $"[QUAZAL Packet Handler] processing x2 {packet.uiSeqId}");
                                ProcessQpacket(packet);
                                ProcessQpacket(sessionlist[i]);
                                sessionlist.Insert(i, packet);
                                for (int it = i; it < sessionlist.Count; it++)
                                {
                                    QPacket pastVal = sessionlist[it - 1];

                                    if (IsCorrectPacket(sessionlist[it], pastVal.uiSeqId))
                                    {
                                        //Log.WriteLine(1, $"[QUAZAL Packet Handler] processing x3 {packet.uiSeqId}");
                                        ProcessQpacket(sessionlist[it]);
                                    }
                                    else if (sessionlist[it].uiSeqId != pastVal.uiSeqId + 1)
                                    {
                                        Log.WriteLine(1, $"[QUAZAL Packet Handler] Found another out of order packet {sessionlist[it].uiSeqId} connid {sessionlist[it].m_bySessionID}");
                                        return;
                                    }
                                }
                                return;

                            }
                            else
                            {
                                //Log.WriteLine(1, $"[QUAZAL Packet Handler] this should not be hapening fix seqid {packet.uiSeqId} prevconnid {sessionlist[i].uiSeqId} connid {packet.m_bySessionID}");
                                return;
                            }
                        }
                        if (sessionlist[i].uiSeqId == sessionlist[i - 1].uiSeqId + 1 && sessionlist[i].packet_began_processing != true)
                        {
                            //Log.WriteLine(1, $"[QUAZAL Packet Handler] Forgot to process packet processing now this should not be happening{packet.uiSeqId} {packet.m_bySessionID}");

                            ProcessQpacket(sessionlist[i]);
                        }
                    }
                    //Log.WriteLine(1, $"[QUAZAL Packet Handler] I is {i} ");
                    if (sessionlist.Count + 1 == packet.uiSeqId)
                    {
                        //Log.WriteLine(1, $"[QUAZAL Packet Handler] Packet arrvied in order processing if01 {packet.uiSeqId} {packet.m_bySessionID}");
                        sessionlist.Add(packet);
                        //Log.WriteLine(1, "Values: " + string.Join(", ", sessionlist.Select(p => p.uiSeqId)));
                        ProcessQpacket(packet);
                        return;

                    }
                    else
                        if (sessionlist.Count + 1 < packet.uiSeqId)
                    {
                        Log.WriteLine(1, $"[QUAZAL Packet Handler] Found out of order packet {packet.uiSeqId} connid {packet.m_bySessionID}");
                        sessionlist.Add(packet);
                        return;
                    }
                    else
                    {
                        //Log.WriteLine(1, $"[QUAZAL Packet Handler] This should not happen {packet.uiSeqId} connid {packet.m_bySessionID} should be {sessionlist.Count + 1}");
                    }

                }



            }
            //start waiter
            return;
        }
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
                //Log.WriteLine(1, $"Logical Processing packet size {p.realSize} sequence id {p.uiSeqId} connection id {p.m_bySessionID} sport {p.m_oSourceVPort} dport {p.m_oDestinationVPort}", Color.Red);
                //HandleQpacket(p);
                lock (_lock)  // Ensure thread-safety
                {
                    ProcessQpacket(p);
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
