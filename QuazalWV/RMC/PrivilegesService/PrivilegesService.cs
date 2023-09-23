using System.Collections.Generic;
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
                    Log.WriteLine(1, "[RMC Privileges] Error: Unknown Method 0x" + rmc.methodID.ToString("X"));
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    var getPrivileges = (RMCPacketRequestPrivilegesService_GetPrivileges)rmc.request;
                    List<Privilege> privileges = DBHelper.GetPrivileges(getPrivileges.LocaleCode);
                    reply = new RMCPacketResponsePrivilegesService_GetPrivileges(privileges);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, "[RMC Privileges] Error: Unknown Method 0x" + rmc.methodID.ToString("X"));
                    break;
            }
        }
    }
}
