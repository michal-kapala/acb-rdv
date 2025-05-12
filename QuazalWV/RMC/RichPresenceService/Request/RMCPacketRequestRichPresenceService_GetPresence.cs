using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestRichPresenceService_GetPresence : RMCPRequest
	{
		public List<uint> Pids {  get; set; }

		public RMCPacketRequestRichPresenceService_GetPresence(Stream s)
		{
			Pids = new List<uint>();
			uint count = Helper.ReadU32(s);
			for (int i = 0; i < count; i++)
				Pids.Add(Helper.ReadU32(s));
            Log.WriteLine(1, $"[RMC GetPresence] GetPresence stuff {string.Join(", ", Pids)}", Color.Red);
        }

		public override string ToString()
		{
			return "[GetPresence Request]";
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteU32(m, (uint)Pids.Count);
			foreach (uint pid in Pids)
				Helper.WriteU32(m, pid);
			return m.ToArray();
		}
	}
}
