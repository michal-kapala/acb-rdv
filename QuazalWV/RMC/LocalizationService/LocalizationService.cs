using System.IO;

namespace QuazalWV
{
    public static class LocalizationService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch(rmc.methodID)
            {
                case 2:
                    rmc.request = new RMCPacketRequestLocalizationService_SetLocaleCode(s);
                    break;
                default:
                    Log.WriteLine(1, "[RMC Localization] Error: Unknown Method 0x" + rmc.methodID.ToString("X"));
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 2:
                    client.LocaleCode = ((RMCPacketRequestLocalizationService_SetLocaleCode)rmc.request).LocalCode;
                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, "[RMC Localization] Error: Unknown Method 0x" + rmc.methodID.ToString("X"));
                    break;
            }
        }

        
    }
}
