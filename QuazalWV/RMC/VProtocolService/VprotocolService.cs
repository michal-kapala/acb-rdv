using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace QuazalWV
{
    public class VprotocolService 
    {
        private static void PrintStreamContent(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            StringBuilder hexBuilder = new StringBuilder();
            // Print each byte as hex
            foreach (byte b in buffer)
            {
                hexBuilder.AppendFormat("{0:X2} ", b);  // Append the byte as 2-digit hex
            }
            Log.WriteLine(1, $"{hexBuilder} ");
                //Console.Write($"{b:X2} ");  // Format as 2-digit hex
            //Console.WriteLine(); // To move to the next line after printing hex
        }
        public static void ProcessRequest(Stream s, RMCP rmc, ClientInfo client)
        {
            switch (rmc.methodID)
            {
                case 1:
                    Log.WriteLine(1, $"[RMC VprotocolService] req ");
                    long currentPosition = s.Position;
                    //PrintStreamContent(s);
                    s.Seek(currentPosition, SeekOrigin.Begin);
                    rmc.request = new RMCPacketRequestVprotocolService_SendVinfo(s,client);
                    
                    Log.WriteLine(1, "[RMC VprotocolService] SendVinfo props:\n" + rmc.request.PayloadToString(), Color.Blue, client);
                    Log.WriteLine(1, $"[RMC VprotocolService] req finished");
                    break;
                default:
                    Log.WriteLine(1, $"[RMC VprotocolService] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            switch (rmc.methodID)
            {
                case 1:
                    Log.WriteLine(1, $"[RMC VprotocolService] sendback");
                    var reqSendVinfo = (RMCPacketRequestVprotocolService_SendVinfo)rmc.request;

                    Log.WriteLine(1, $"[RMC VprotocolService] VirginProtocol {reqSendVinfo.PayloadToString()}", Color.Red, client);
                    Log.WriteLine(1, $"[RMC VprotocolService] sendback finished");

                    reply = new RMCPResponseEmpty();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                
                default:
                    Log.WriteLine(1, $"[RMC VprotocolService] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }
    }
}
