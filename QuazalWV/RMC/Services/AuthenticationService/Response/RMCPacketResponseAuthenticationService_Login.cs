using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices.Expando;
using System.Text;
namespace QuazalWV
{
    /// <summary>
    /// https://github.com/kinnay/NintendoClients/wiki/NEX-Common-Types#rvconnectiondata-structure
    /// </summary>
    public class RVConnectionData
    {
        public string m_urlRegularProtocols = "prudps:/address=#ADDRESS#;port=#PORT#;CID=1;PID=#SERVERID#;sid=1;stream=3;type=2";
        public List<byte> m_lstSpecialProtocols = new List<byte>();
        public string m_urlSpecialProtocols = "";
    }
    public class RMCPacketResponseAuthenticationService_Login : RMCPResponse
    {
        public uint resultCode;
        public uint PID;
        public byte[] ticketBuffer;
        public KerberosTicket ticket;
        public string address;
        public ushort port;
        public uint serverID = 2;
        public RVConnectionData rvConnData = new RVConnectionData();
        public string returnMsgServerBuild;
        /// <summary>
        /// SP ticket dumped from the original traffic.
        /// </summary>
        public byte[] spTrackingUserTgt = { 0x22, 0x17, 0x39, 0x1D, 0x8D, 0x7C, 0xFC, 0xFB, 0xC8, 0x02, 0xE6, 0x01, 0x92, 0xC9, 0xDA, 0xDD, 0xB8, 0x57, 0xD0, 0x15, 0xA7, 0xA1, 0xAB, 0x03, 0x57, 0x3F, 0xC1, 0x69, 0x4D, 0xA0, 0x23, 0x83, 0x14, 0xCC, 0xDE, 0x16, 0x42, 0x37, 0xCF, 0x66, 0x8B, 0x21, 0xEC, 0x71, 0xF4, 0xB9, 0x07, 0x49, 0x92, 0x32, 0x81, 0x21, 0x5F, 0x78, 0x37, 0xCE, 0x7A, 0x9A, 0x68, 0x75, 0x8E, 0x8E, 0xD4, 0xDA, 0x32, 0xE3, 0x44, 0x5E, 0xAE, 0x2E, 0x89, 0xF9, 0x1E, 0xF8, 0x6D, 0x24 };
        /// <summary>
        /// MP ticket dumped from the original traffic.
        /// </summary>
        public byte[] mpTrackingUserTgt = { 0xB6, 0xE1, 0xA2, 0xED, 0xD8, 0x7E, 0xCB, 0x5F, 0xEA, 0x36, 0xFF, 0x97, 0x75, 0xB3, 0xD8, 0xCB, 0xB8, 0x57, 0xD0, 0x15, 0xA7, 0xA1, 0xAB, 0x03, 0x57, 0x3F, 0xC1, 0x69, 0xD9, 0x56, 0xB8, 0x73, 0x41, 0xCE, 0xE9, 0xB2, 0x60, 0x03, 0xD6, 0xF0, 0x6C, 0x5B, 0xEE, 0x67, 0xFA, 0x6F, 0x7E, 0x5B, 0xEF, 0xB7, 0xBB, 0x62, 0x69, 0xAF, 0x69, 0x7A, 0xD8, 0x69, 0x58, 0x9A, 0x60, 0x4B, 0xA4, 0xA7, 0x4F, 0x41, 0xBF, 0x80, 0xC7, 0x14, 0x1A, 0x5F, 0x35, 0x0F, 0xFD, 0x96 };
        public RMCPacketResponseAuthenticationService_Login(ClientInfo client)
        {
            resultCode = 0x10001;

            if (Global.IsPrivate)
            {
                // Use local IP for the private server
                address = GetConfigAddress();
            }
            else
            {
                try
                {
                    // Get public IP of external service
                    using (var web = new System.Net.WebClient())
                    {
                        address = web.DownloadString("https://api.ipify.org").Trim();
                    }
                }
                catch
                {
                    // Fallback to config IP if the request fails
                    address = GetConfigAddress();
                }
            }

            // Only Tracking user calls Login
            PID = client.TrackingUser.Pid;
            port = client.sPort;
            // unused
            ticket = new KerberosTicket(serverID)
            {
                userPID = PID,
                sessionKey = new byte[]
                {
            0x9C, 0xB0, 0x1D, 0x7A, 0x2C, 0x5A, 0x6C, 0x5B,
            0xED, 0x12, 0x68, 0x45, 0x69, 0xAE, 0x09, 0x0D
                },
                ticket = new byte[]
                {
            0x76, 0x21, 0x4B, 0xA6, 0x21, 0x96, 0xD3, 0xF3,
            0x9A, 0x8C, 0x7A, 0x27, 0x0D, 0xD9, 0xB3, 0xFA,
            0x21, 0x0E, 0xED, 0xAF, 0x42, 0x63, 0x92, 0x95,
            0xC1, 0x16, 0x54, 0x08, 0xEE, 0x6E, 0x69, 0x17,
            0x35, 0x78, 0x2E, 0x6E
                }
            };
            ticketBuffer = mpTrackingUserTgt;
            returnMsgServerBuild = "";
            rvConnData.m_urlRegularProtocols = rvConnData.m_urlRegularProtocols
                .Replace("#ADDRESS#", address)
                .Replace("#PORT#", port.ToString())
                .Replace("#SERVERID#", serverID.ToString());
        }
        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, resultCode);
            Helper.WriteU32(m, PID);
            // kerberos ticket
            Helper.WriteU32(m, (uint)ticketBuffer.Length);
            m.Write(ticketBuffer, 0, ticketBuffer.Length);
            // secure server url
            Helper.WriteString(m, rvConnData.m_urlRegularProtocols);
            Helper.WriteU32(m, (uint)rvConnData.m_lstSpecialProtocols.Count);
            // special protocols list
            foreach (byte b in rvConnData.m_lstSpecialProtocols)
                Helper.WriteU8(m, b);
            // special protocols url
            Helper.WriteString(m, rvConnData.m_urlSpecialProtocols);
            // server build, doesnt need to be sent
            Helper.WriteString(m, returnMsgServerBuild);
            return m.ToArray();
        }
        public override string ToString()
        {
            return "[Authentication Service Login Response]";
        }
        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            string secServerUrl = rvConnData.m_urlRegularProtocols;
            sb.AppendLine("\t[Result code       : 0x" + resultCode.ToString("X8") + "]");
            sb.AppendLine("\t[PID               : 0x" + PID.ToString("X8") + "]");
            sb.AppendLine(ticket.ToString());
            sb.AppendLine("\t[Secure Server URL : " + secServerUrl + "]");
            return sb.ToString();
        }
        /// <summary>
        /// Returns SecureServerAddress setting defined in 'TTLBackend.exe.config'.
        /// </summary>
        /// <returns></returns>
        private string GetConfigAddress()
        {
            return Global.ServerBindAddress;
        }
    }
}