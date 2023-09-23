using System.IO;

namespace QuazalWV
{
    public static class UplayWinService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 5:
                    rmc.request = new RMCPacketRequestUplayWinService_GetRewards(s);
                    break;
                default:
                    Log.WriteLine(1, "[RMC UplayWin] Error: Unknown Method 0x" + rmc.methodID.ToString("X"));
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 5:
                    var getRewards = (RMCPacketRequestUplayWinService_GetRewards)rmc.request;
                    var rewards = DBHelper.GetRewards(getRewards.PlatformCode);
                    reply = new RMCPacketResponseUplayWinService_GetRewards(rewards);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, "[RMC UplayWin] Error: Unknown Method 0x" + rmc.methodID.ToString("X"));
                    break;
            }
        }
    }
}
