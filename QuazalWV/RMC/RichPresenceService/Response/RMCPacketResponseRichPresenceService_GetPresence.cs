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
            //byte[] byteArray = new byte[] { 0x0,0x0,0x0 };
			//Elements.Add(new PresenceElement(1, 1, false, byteArray));

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
