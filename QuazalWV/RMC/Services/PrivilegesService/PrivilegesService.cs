using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public class PrivilegesService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestPrivilegesService_GetPrivileges(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Privileges] Error: Unknown Method {rmc.methodID}", Color.Red);
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
                    Log.WriteLine(1, $"[RMC Privileges] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
