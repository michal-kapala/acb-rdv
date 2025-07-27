using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseAuthenticationService_LoginEx : RMCPResponse
    {
        public uint resultCode = 0x10001;
        public uint PID;
        public byte[] pbufResponse;
        public KerberosTicket ticket;
        public string address = Global.ServerBindAddress;
        public ushort port;
        public uint serverID;
        public string pConnectionData = "prudps:/address=#ADDRESS#;port=#PORT#;CID=1;PID=#SERVERID#;sid=1;stream=3;type=2";
        public uint specialProtoCount = 0;
        public string specialProtoUrl = "";
        public string returnMsg = "";
        
        public RMCPacketResponseAuthenticationService_LoginEx(uint pid, uint sPID, ushort sPort)
        {
            PID = pid;
            serverID = sPID;
            port = sPort;
            ticket = new KerberosTicket(serverID)
            {
                userPID = PID,
                sessionKey = new byte[] { 0x9C, 0xB0, 0x1D, 0x7A, 0x2C, 0x5A, 0x6C, 0x5B, 0xED, 0x12, 0x68, 0x45, 0x69, 0xAE, 0x09, 0x0D },
                ticket = new byte[] { 0x76, 0x21, 0x4B, 0xA6, 0x21, 0x96, 0xD3, 0xF3, 0x9A, 0x8C, 0x7A, 0x27, 0x0D, 0xD9, 0xB3, 0xFA, 0x21, 0x0E, 0xED, 0xAF, 0x42, 0x63, 0x92, 0x95, 0xC1, 0x16, 0x54, 0x08, 0xEE, 0x6E, 0x69, 0x17, 0x35, 0x78, 0x2E, 0x6E }
            };
            pbufResponse = ticket.ToBuffer();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, resultCode);
            Helper.WriteU32(m, PID);
            Helper.WriteU32(m, (uint)pbufResponse.Length);
            m.Write(pbufResponse, 0, pbufResponse.Length);
            string regularUrl = MakeConnectionString();
            Helper.WriteString(m, regularUrl);
            Helper.WriteU32(m, specialProtoCount);
            Helper.WriteString(m, specialProtoUrl);
            Helper.WriteString(m, returnMsg);
            return m.ToArray();
        }

        private string MakeConnectionString()
        {
            return pConnectionData.Replace("#ADDRESS#", address).Replace("#PORT#", port.ToString()).Replace("#SERVERID#", serverID.ToString());
        }

        public override string ToString()
        {
            return "[LoginCustomData Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\t[Result code       : 0x" + resultCode.ToString("X8") + "]");
            sb.AppendLine("\t[PID               : 0x" + PID.ToString("X8") + "]");
            sb.AppendLine(ticket.ToString());
            sb.AppendLine("\t[RVConnectionData  : " + MakeConnectionString() + "]");
            sb.AppendLine("\t[Special protocols : 0x" + specialProtoCount.ToString("X8") + "]");
            return sb.ToString();
        }
    }
}
