using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
        public uint serverID;
        public RVConnectionData rvConnData = new RVConnectionData();
        public string returnMsgServerBuild;

        public RMCPacketResponseAuthenticationService_Login()
        {

        }

        public RMCPacketResponseAuthenticationService_Login(ClientInfo client)
        {
            resultCode = 0x10001;
            // Address for the secure server is configurable in TTLBackend.exe.config
            address = client.isLocal ? "127.0.0.1" : GetConfigAddress();
            PID = client.PID;
            serverID = client.sPID;
            port = client.sPort;
            ticket = new KerberosTicket(serverID)
            {
                userPID = PID,
                sessionKey = new byte[] { 0x9C, 0xB0, 0x1D, 0x7A, 0x2C, 0x5A, 0x6C, 0x5B, 0xED, 0x12, 0x68, 0x45, 0x69, 0xAE, 0x09, 0x0D },
                ticket = new byte[] { 0x76, 0x21, 0x4B, 0xA6, 0x21, 0x96, 0xD3, 0xF3, 0x9A, 0x8C, 0x7A, 0x27, 0x0D, 0xD9, 0xB3, 0xFA, 0x21, 0x0E, 0xED, 0xAF, 0x42, 0x63, 0x92, 0x95, 0xC1, 0x16, 0x54, 0x08, 0xEE, 0x6E, 0x69, 0x17, 0x35, 0x78, 0x2E, 0x6E }
            };
            ticketBuffer = ticket.toBuffer();
            returnMsgServerBuild = "the server build string";
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
            return ConfigurationManager.AppSettings["SecureServerAddress"];
        }
    }
}