using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace QuazalWV
{
    public static class MatchMakingService
    {
        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 16:
                    reply = new RMCPacketResponseMatchMakingService_GetParticipantsURLs();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC MatchMakingService] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
