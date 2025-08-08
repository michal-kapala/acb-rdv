using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public class PrivilegesService
    {
        public const RMCP.PROTOCOL protocol = RMCP.PROTOCOL.Privileges;

        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestPrivilegesService_GetPrivileges(s);
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
                case 1:
                    var getPrivileges = (RMCPacketRequestPrivilegesService_GetPrivileges)rmc.request;
                    List<Privilege> privileges = DbHelper.GetPrivileges(getPrivileges.LocaleCode);
                    reply = new RMCPacketResponsePrivilegesService_GetPrivileges(privileges);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteRmcLine(1, $"Error: Unknown Method {rmc.methodID}", protocol, LogSource.RMC, Color.Red, client);
                    break;
            }
        }
    }
}
