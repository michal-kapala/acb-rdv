using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
namespace QuazalWV
{
    public class ClientInfo
    {
        public static System.Random rand = new System.Random();
        public uint PID { get; set; }
        public uint sPID;
        public ushort sPort;
        public uint IDrecv;
        public uint rvCID = (uint)rand.Next();
        public uint IDsend;
        public byte sessionID;
        public byte[] sessionKey = new byte[] { 0x9C, 0xB0, 0x1D, 0x7A, 0x2C, 0x5A, 0x6C, 0x5B, 0xED, 0x12, 0x68, 0x45, 0x69, 0xAE, 0x09, 0x0D };
        public ushort gameSeqId;
        /// <summary>
        /// Reliable substream sequence ID.
        /// </summary>
        public ushort seqIdReliable = 1;
        /// <summary>
        /// Unreliable substream sequence ID.
        /// </summary>
        public ushort seqIdUnreliable = 1;
        /// <summary>
        /// Reliable substream sequence ID for Tracking user.
        /// </summary>
        public ushort seqIdReliableTracking = 1;
        /// <summary>
        /// Unreliable substream sequence ID for Tracking user.
        /// </summary>
        public ushort seqIdUnreliableTracking = 1;
        /// <summary>
        /// Client's PRUDP signature used by the player.
        /// </summary>
        public uint playerSignature;
        /// <summary>
        /// Client's PRUDP signature used by Tracking user.
        /// </summary>
        public uint trackingSignature;
        public uint callCounterRMC;
        public IPEndPoint ep;
        public UdpClient udp;
        public List<StationUrl> RegisteredUrls { get; set; } = new List<StationUrl>();
        public List<StationUrl> Urls { get; set; } = new List<StationUrl>();
        public User User { get; set; }
        public User TrackingUser { get; set; }
        public string LocaleCode { get; set; }
        public List<string> TrackingUserUrls { get; set; } = new List<string>();
        /// <summary>
        /// Current game session ID.
        /// </summary>
        public uint GameSessionID { get; set; } = 0;
        public bool InGameSession { get; set; } = false;
        public uint AbandonedSessionID { get; set; } = 0;
        public bool AbandoningSession { get; set; } = false;
        /// <summary>
        /// Rich presence info.
        /// </summary>
        public List<PresenceProperty> PresenceProps { get; set; } = new List<PresenceProperty>();
    }
}
