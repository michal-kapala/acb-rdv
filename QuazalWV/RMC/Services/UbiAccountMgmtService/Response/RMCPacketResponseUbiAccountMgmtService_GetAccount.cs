using System.Drawing;
using System.IO;
using System.Text;

namespace QuazalWV
{
    class RMCPacketResponseUbiAccountMgmtService_GetAccount : RMCPResponse
    {
        public UbiAccount UbiAccount { get; set; }
        public bool Exists { get; set; }

        public RMCPacketResponseUbiAccountMgmtService_GetAccount(User user)
        {
            if (user != null)
            {
                UbiAccount = new UbiAccount()
                {
                    UbiAccountId = user.UbiId,
                    Username = user.Name,
                    Email = user.Email,
                    CountryCode = user.CountryCode,
                };
                Exists = true;
            }
            else
                Log.WriteRmcLine(1, "GetAccount got a null user", RMCP.PROTOCOL.UbiAccountMgmt, LogSource.RMC, Color.Red);
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[Account: {UbiAccount.Username}]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            UbiAccount.ToBuffer(m);
            Helper.WriteBool(m, Exists);
            return m.ToArray();
        }

        public override string ToString()
        {
            return "[GetAccount Response]";
        }
    }
}
