using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
    public class RMCPacketResponseRichPresenceService_GetPresence : RMCPResponse
    {
        public List<PresenceElement> Elements {  get; set; }

        public RMCPacketResponseRichPresenceService_GetPresence()
        {
            Elements = new List<PresenceElement>();
        }

        public override string ToString()
        {
            return "[GetPresence Response]";
        }

        public override string PayloadToString()
        {
            return "";
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Helper.WriteU32(m, (uint)Elements.Count);
            foreach (PresenceElement element in Elements)
            element.ToBuffer(m);
            return m.ToArray();
        }
    }
}
