using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace QuazalWV
{
    public static class SecureService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 1:
                    rmc.request = new RMCPacketRequestSecureService_Register(s);

                    break;
                case 4:
                    rmc.request = new RMCPacketRequestSecureService_RegisterEx(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Secure] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }


        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    var reqRegister = (RMCPacketRequestSecureService_Register)rmc.request;
                    // Change ip addresses to external and save urls
                    reqRegister.SetUrls(client);
                    reply = new RMCPacketResponseSecureService_Register(client);
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                case 4:
                    var reqRegisterEx = (RMCPacketRequestSecureService_RegisterEx)rmc.request;
                    switch (reqRegisterEx.className)
                    {
                        case "UbiAuthenticationLoginCustomData":
                            reply = new RMCPacketResponseRegisterEx(client.PID);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                            break;
                        default:
                            Log.WriteLine(1, $"[RMC Secure] Error: Unknown Custom Data class {reqRegisterEx.className}", Color.Red, client);
                            break;
                    }
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Secure] Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
