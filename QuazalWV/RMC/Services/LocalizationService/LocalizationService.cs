using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public static class LocalizationService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.Localization;

        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch(rmc.methodID)
            {
                case 2:
                    rmc.request = new RMCPacketRequestLocalizationService_SetLocaleCode(s);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red);
                    break;
            }
        }

        public static void HandleRequest(PrudpPacket p, RMCP rmc, ClientInfo client)
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
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }

    }
}
