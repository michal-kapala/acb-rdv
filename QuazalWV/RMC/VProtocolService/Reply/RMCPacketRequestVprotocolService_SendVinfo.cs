using System.IO;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System;
using System.Runtime.InteropServices.ComTypes;

namespace QuazalWV
{
    internal class RMCPacketRequestVprotocolService_SendVinfo : RMCPRequest
    {
        private Stream s;
        uint client_pid;
        uint pid;
        ulong sessionID;
        Session session;
        ClientInfo client;
        QDateTime begin;
        QDateTime end;
        bool host;
        bool hasQuit;
        uint uniquePlayerMax;
        uint gameMode;
        byte platform;
        uint score;
        uint killNumber;
        public override string PayloadToString()
        {
            try
            {

                StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Vprotocol Dump]");
            sb.AppendLine($"\t[SessionID: {sessionID}]");
            sb.AppendLine($"\t[Client sender: {client.User.Name}]");
            if (begin.isvalid)
                {
                    sb.AppendLine($"\t[Date begin: {begin.Time.ToString()}]");
                }
                else
                {
                    sb.AppendLine($"\t[Invalid  date ]");
                }
            if (end.isvalid)
                {
                    sb.AppendLine($"\t[Date end: {end}]");
            }
                else
            {
                sb.AppendLine($"\t[Invalid end date ]");
            }
            sb.AppendLine($"\t[host_bool: {host}]");
            sb.AppendLine($"\t[hasQuit: {hasQuit}]");
            sb.AppendLine($"\t[uniquePlayerMax: {uniquePlayerMax}]");
            sb.AppendLine($"\t[Game Mode: { gameMode }]"); //System.Enum.GetName(typeof(GameMode)
            sb.AppendLine($"\t[platform: {platform}]");
            sb.AppendLine($"\t[score: {score}]");
            sb.AppendLine($"\t[killnumber: {killNumber}]");
            return sb.ToString();

            }
            catch (Exception ex)
            {
                // Catch the exception and print the error message
                Log.WriteLine(1, $"Error: {ex.Message}");
                // Optionally, you can print the stack trace or other details
                Log.WriteLine(1, $"Stack Trace: {ex.StackTrace}");
            }
            return "ERROR";
        }
        public RMCPacketRequestVprotocolService_SendVinfo(Stream s, ClientInfo sclient)
        {
            try
            {

            this.client = sclient;
            this.s = s;
            pid = Helper.ReadU32(s);
            sessionID = Helper.ReadU64(s);
            begin = new QDateTime(s);
            end = new QDateTime(s);
            host = Helper.ReadBool(s);
            hasQuit = Helper.ReadBool(s);
            uniquePlayerMax=Helper.ReadU32(s);
            gameMode = Helper.ReadU8(s);
            platform = Helper.ReadU8(s);
            score = Helper.ReadU8(s);
            killNumber = Helper.ReadU8(s);
            }
            catch (Exception ex)
            {
                // Catch the exception and print the error message
                Log.WriteLine(1, $"[RMC VprotocolService] error msg {ex.Message} ");
                //Console.WriteLine($"Error: {ex.Message}");
                // Optionally, you can print the stack trace or other details
                Log.WriteLine(1, $"[RMC VprotocolService] error msg {ex.StackTrace} ");
                //Console.WriteLine($"Stack Trace: ");
            }
        }
        public override string ToString()
        {
            return "[RMCPacketRequestVprotocolService_SendVinfo Request] ";
        }
        
        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            return m.ToArray();
        }

    }

}