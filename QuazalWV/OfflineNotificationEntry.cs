using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;

namespace QuazalWV
{
    public class OfflineNotificationEntry
    {
        
        public uint source;
        public uint type;
        public uint subType;
        public uint param1;
        public uint param2;
        public uint param3;
        public string paramStr;

        public OfflineNotificationEntry( uint src, uint type, uint subtype, uint parameter1, uint parameter2, uint parameter3, string parameterString)
        {
            
            source = src;
            this.type = type;
            this.subType = subtype;
            this.param1 = parameter1;
            this.param2 = parameter2;
            this.param3 = parameter3;
            this.paramStr = parameterString;
        }

       
    }
}
