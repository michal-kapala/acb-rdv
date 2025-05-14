using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;

namespace QuazalWV
{
    public class NotificationQueueEntry
    {
        public ClientInfo client;
        public Stopwatch timer;
        public uint timeout;
        public uint source;
        public uint type;
        public uint subType;
        public uint param1;
        public uint param2;
        public uint param3;
        public string paramStr;

        public NotificationQueueEntry(ClientInfo client, uint time, uint src, uint type, uint subtype, uint parameter1, uint parameter2, uint parameter3, string parameterString)
        {
            this.client = client;
            timer = new Stopwatch();
            timer.Start();
            timeout = time;
            source = src;
            this.type = type;
            this.subType = subtype;
            this.param1 = parameter1;
            this.param2 = parameter2;
            this.param3 = parameter3;
            this.paramStr = parameterString;
        }
        public NotificationQueueEntry(OfflineNotificationEntry offlinenotif , ClientInfo client, uint time )
        {
            this.client = client;
            timer = new Stopwatch();
            timer.Start();
            timeout = time;
            this.source = offlinenotif.source;
            this.type = offlinenotif.type;
            this.subType = offlinenotif.subType;
            this.param1 = offlinenotif.param1;
            this.param2 = offlinenotif.param2;
            this.param3 = offlinenotif.param3;
            this.paramStr = offlinenotif.paramStr;
        }
        public override string ToString()
        {
            return $"Notification type {type} subtype {subType} par1 {param1} par2 {param2} par3 {param3} paramstr {paramStr}";
        }
        public void Execute()
        {
            RMC.SendNotification(client, source, type, subType, param1, param2, param3, paramStr);
        }
    }
}
