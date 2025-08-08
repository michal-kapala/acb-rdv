using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCP
    {
        public enum PROTOCOL
        {
            RemoteLogDevice = 1,
            NatTraversalRelay = 3,            
            Authentication = 0xA,
            Secure = 0xB,
            GlobalNotificationEvent = 0xE,
            Health = 0x12,
            Monitoring = 0x13,
            Friends = 0x14,
            MatchMaking = 0x15,
            Messaging = 0x17,
            PersistentStore = 0x18,
            AccountMgmt = 0x19,
            MessageDelivery = 0x1B,
            UbiAccountMgmt = 0x1D,
            News = 0x1F,
            NewsAdmin = 0x20,
            UbiNews = 0x21,
            Privileges = 0x23,
            Tracking = 0x24,
            Localization = 0x27,
            LocalizationAdmin = 0x28,
            GameSession = 0x2A,
            GameSessionAdmin = 0x2B,
            MatchMakingExt = 0x32,
            SinglePlayerStats = 0x65,
            HermesPlayerStats = 0x6C,
            RichPresence = 0x6D,
            Clans = 0x6E,
            TrackingExt = 0x6F,
            MetaSession = 0x70,
            GameInfo = 0x71,
            ContactsExtensions = 0x72,
            HermesAchievements = 0x74,
            SocialNetworks = 0x75,
            Virgin = 0x76,
            Ac2RomeLeaderboard = 0x77,
            UplayWin = 0x78,
            AcbProxyGameProfile = 0x79,
            ShopRenting = 0x7A,
        }

        public PROTOCOL proto;
        public bool isRequest;
        public bool success;
        public uint error;
        public uint callID;
        public uint methodID;
        public RMCPRequest request;
        public int _afterProtocolOffset;

        public RMCP()
        {
        }

        public RMCP(PrudpPacket p)
        {
            MemoryStream m = new MemoryStream(p.payload);
            Helper.ReadU32(m);
            ushort b = Helper.ReadU8(m);
            isRequest = (b >> 7) == 1;
            try
            {
                if ((b & 0x7F) != 0x7F)
                    proto = (PROTOCOL)(b & 0x7F);
                else
                {
                    b = Helper.ReadU16(m);
                    proto = (PROTOCOL)(b);
                }
            }
            catch
            {
                Log.WriteLine(1, $"Error: Unknown RMC packet protocol 0x{b:X2}", LogSource.RMC);
                return;
            }
            _afterProtocolOffset = (int)m.Position;
        }
        

        public override string ToString()
        {
            string result = success ? "Success" : "Fail";
            string res = isRequest ? "" : $" Result={result}";
            return $"[RMC Packet : Proto = {proto} CallID={callID} MethodID={methodID}{res}]";
        }

        public string PayLoadToString()
        {
            StringBuilder sb = new StringBuilder();
            if (request != null)
                sb.Append(request);
            return sb.ToString();
        }

        public byte[] ToBuffer()
        {
            MemoryStream result = new MemoryStream();
            byte[] buff = request.ToBuffer();
            Helper.WriteU32(result, (uint)(buff.Length + 9));
            byte b = (byte)proto;
            if (isRequest)
                b |= 0x80;
            Helper.WriteU8(result, b);
            Helper.WriteU32(result, callID);
            Helper.WriteU32(result, methodID);
            result.Write(buff, 0, buff.Length);
            return result.ToArray();
        }
    }
}
