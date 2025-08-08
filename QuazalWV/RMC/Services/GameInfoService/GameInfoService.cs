using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public static class GameInfoService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 5:
                    rmc.request = new RMCPacketRequestGameInfoService_GetFileListOnGameStorage(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Game Info] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(PrudpPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 5:
                    var rGetFiles = (RMCPacketRequestGameInfoService_GetFileListOnGameStorage)rmc.request;
                    reply = new RMCPacketResponseGameInfoService_GetFileListOnGameStorage(rGetFiles.FileName);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Game Info] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
