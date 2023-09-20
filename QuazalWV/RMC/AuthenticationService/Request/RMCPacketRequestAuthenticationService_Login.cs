using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketRequestAuthenticationService_Login : RMCPRequest
    {
        public string username;

        public RMCPacketRequestAuthenticationService_Login()
        {
        }

        public RMCPacketRequestAuthenticationService_Login(Stream s)
        {
            username = Helper.ReadString(s);
        }

        public override byte[] ToBuffer()
        {
            MemoryStream result = new MemoryStream();
            Helper.WriteString(result, username);
            return result.ToArray();
        }

        public override string ToString()
        {
            return "[Login Request : username=" + username + "]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\t[Username   : " + username + "]");
            return "";
        }
    }
}