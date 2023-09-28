using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace QuazalWV
{
    public class ClientInfo
    {
        public uint PID;
        public uint sPID;
        public ushort sPort;
        public uint IDrecv;
        public uint IDsend;
        public byte sessionID;
        public byte[] sessionKey;
        public ushort seqCounter;
        public uint callCounterRMC;
        public IPEndPoint ep;
        public UdpClient udp;
        public bool isLocal = true;
        public User User { get; set; }
        public User TrackingUser { get; set; }
        public string LocaleCode { get; set; }
        public List<string> TrackingUserUrls { get; set; } = new List<string>();
    }
}
