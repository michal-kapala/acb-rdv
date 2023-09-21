using System.IO;

namespace QuazalWV
{
    public static class UbiAccountMgmtService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 3:
                    // Empty GetAccount request
                    break;
                default:
                    Log.WriteLine(1, "[RMC UbiAccountMgmt] Error: Unknown Method 0x" + rmc.methodID.ToString("X"));
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 3:
                    reply = new RMCPacketResponseUbiAccountMgmtService_GetAccount(client.User);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, "[RMC UbiAccountMgmt] Error: Unknown Method 0x" + rmc.methodID.ToString("X"));
                    break;
            }
        }
    }
}
