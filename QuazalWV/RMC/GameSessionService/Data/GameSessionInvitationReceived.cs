using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuazalWV
{
    public class GameSessionInvitationReceived : IData
    {
        public GameSessionKey SessionKey { get; set; }
        public uint SenderPid { get; set; }
        public string Message { get; set; }
        public QDateTime CreationTime { get; set; }

        public GameSessionInvitationReceived()
        {
            SessionKey = new GameSessionKey();
            SenderPid = 2;
            Message = "join pls";
            CreationTime = new QDateTime(DateTime.Now);
        }

        public GameSessionInvitationReceived(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            SessionKey = new GameSessionKey(s);
            SenderPid = Helper.ReadU32(s);
            Message = Helper.ReadString(s);
            CreationTime = new QDateTime(s);
        }

        public void ToBuffer(Stream s)
        {
            throw new NotImplementedException();
        }
    }
}
