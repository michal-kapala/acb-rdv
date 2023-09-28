using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class GameSessionService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 14:
                    rmc.request = new RMCPacketRequestGameSessionService_GetInvitationsReceived(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC GameSession] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 14:
                    var getRecvInvites = (RMCPacketRequestGameSessionService_GetInvitationsReceived)rmc.request;
                    reply = new RMCPacketResponseGameSessionService_GetInvitationsReceived();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC GameSession] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
