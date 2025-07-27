using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCP
    {
        public enum PROTOCOL
        {
            RemoteLogDeviceService = 1,
            NATTraversalRelayService = 3,            
            AuthenticationService = 0xA,
            SecureService = 0xB,
            GlobalNotificationEventService = 0xE,
            HealthService = 0x12,
            MonitoringService = 0x13,
            FriendsService = 0x14,
            MatchMakingService = 0x15,
            MessagingService = 0x17,
            PersistentStoreService = 0x18,
            AccountMgmtService = 0x19,
            MessageDeliveryService = 0x1B,
            UbiAccountMgmtService = 0x1D,
            NewsService = 0x1F,
            NewsAdminService = 0x20,
            UbiNewsService = 0x21,
            PrivilegesService = 0x23,
            TrackingService = 0x24,
            LocalizationService = 0x27,
            LocalizationAdminService = 0x28,
            GameSessionService = 0x2A,
            GameSessionAdminService = 0x2B,
            MatchMakingExtService = 0x32,
            SinglePlayerStatsService = 0x65,
            HermesPlayerStatsService = 0x6C,
            RichPresenceService = 0x6D,
            ClansService = 0x6E,
            TrackingExtService = 0x6F,
            MetaSessionService = 0x70,
            GameInfoService = 0x71,
            ContactsExtensionsService = 0x72,
            HermesAchievementsService = 0x74,
            SocialNetworksService = 0x75,
            VirginService = 0x76,
            Ac2RomeLeaderboardService = 0x77,
            UplayWinService = 0x78,
            AcbProxyGameProfileService = 0x79,
            ShopRentingService = 0x7A,
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

        public RMCP(QPacket p)
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
                Log.WriteLine(1, "[RMC Packet] Error: Unknown RMC packet protocol 0x" + b.ToString("X2"));
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
