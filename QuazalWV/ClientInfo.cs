using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;

//TODO sign out if the same client signs in

namespace QuazalWV
{
    public class ClientInfo
    {
        public static System.Random rand = new System.Random();
        public uint ServerIncrementedGeneratedPID;
        public uint ServerStaticID;
        public ushort sPort;
        public uint clientServerGeneratedConnectionSig;
        public uint GameSessionID;
        public bool InGameSession = false;
        public uint rvCID = (uint)rand.Next();
        public uint IDsend;
        public byte sessionID;
        public bool toabandon = false;
        public uint abandonID;
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
        public bool isLocal = true;
        public User User { get; set; }
        public User TrackingUser { get; set; }
        public string LocaleCode { get; set; }
        public List<string> TrackingUserUrls { get; set; } = new List<string>();
        public IPAddress IPaddress { get; internal set; }
        public bool Playersignout =false;
        public bool Trackingsignout = false;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($" gamesessid {GameSessionID} user {User} sessionID {sessionID} pid {ServerIncrementedGeneratedPID} spid {ServerStaticID} ");
            return sb.ToString();
        }
    }
}
