using System.Diagnostics;

namespace QuazalWV
{
    public class NotificationEvent
    {
        public ClientInfo Client { get; set; }
        public Stopwatch Timer { get; set; }
        public uint Timeout {get; set;}
        public uint Source { get; set; }
        public uint Type { get; set; }
        public uint Subtype { get; set; }
        public uint Param1 { get; set; }
        public uint Param2 { get; set; }
        public uint Param3 { get; set; }
        public string ParamStr { get; set; }

        public NotificationEvent(ClientInfo c, uint timeout, uint source, uint type, uint subtype, uint param1, uint param2, uint param3, string paramStr)
        {
            Client = c;
            Timer = new Stopwatch();
            Timer.Start();
            Timeout = timeout;
            Source = source;
            Type = type;
            Subtype = subtype;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            ParamStr = paramStr;
        }

        public void Send()
        {
            RMC.SendNotification(Client, Source, Type, Subtype, Param1, Param2, Param3, ParamStr);
        }
    }
}
