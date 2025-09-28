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
        public byte[] sessionKey = new byte[]
        {
            0x9C, 0xB0, 0x1D, 0x7A, 0x2C, 0x5A, 0x6C, 0x5B,
            0xED, 0x12, 0x68, 0x45, 0x69, 0xAE, 0x09, 0x0D
        };
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
        public List<PresenceProperty> PresenceProps { get; set; } = new List<PresenceProperty>();
        /// <summary>
        /// Resolved IP address used for communication.
        /// </summary>
        public string ResolvedIp { get; set; }

        /// <summary>
        /// Determines the address the client should use for communication.
        /// Uses the local IP if the server is private/localhost, otherwise fetches the public IP.
        /// </summary>
        public void DetermineConnectionAddress()
        {
            byte[] bytes = ep.Address.GetAddressBytes();
            bool isPrivate = (bytes[0] == 10) ||
                             (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                             (bytes[0] == 192 && bytes[1] == 168) ||
                             (bytes[0] == 127);

            if (isPrivate)
            {
                ResolvedIp = ep.Address.ToString();
            }
            else
            {
                try
                {
                    // Fetch public IP of the client
                    ResolvedIp = new System.Net.WebClient().DownloadString("https://api.ipify.org").Trim();
                }
                catch
                {
                    // Fallback to local IP if public is unreachable
                    ResolvedIp = ep.Address.ToString();
                }
            }
        }
    }
}
