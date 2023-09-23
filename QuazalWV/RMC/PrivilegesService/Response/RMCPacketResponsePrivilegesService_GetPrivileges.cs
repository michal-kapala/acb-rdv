using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponsePrivilegesService_GetPrivileges : RMCPResponse
    {
        public Dictionary<uint, Privilege> Privileges { get; set; }

        public RMCPacketResponsePrivilegesService_GetPrivileges(List<Privilege> privileges)
        {
            Privileges = ToDict(privileges);
        }

        public override string ToString()
        {
            return "[GetPrivileges Response]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<uint, Privilege> p in Privileges)
                sb.AppendLine($"\t[Privilege: {p.Value} ({p.Key})]");
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Privileges.Count);
            foreach (KeyValuePair<uint, Privilege> p in Privileges)
            {
                Helper.WriteU32(m, p.Key);
                p.Value.ToBuffer(m);
            }
            return m.ToArray();
        }

        private Dictionary<uint, Privilege> ToDict(List<Privilege> privileges)
        {
            Dictionary<uint, Privilege> dict = new Dictionary<uint, Privilege>();
            foreach (Privilege p in privileges)
                dict.Add(p.Id, p);
            return dict;
        }
    }
}
