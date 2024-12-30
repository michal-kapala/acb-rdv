using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestRichPresenceService_SetPresence : RMCPRequest
	{
		public uint Pid { get; set; }
		public QBuffer Buffer { get; set; }

		public RMCPacketRequestRichPresenceService_SetPresence(Stream s)
		{
			Pid = Helper.ReadU32(s);
			Buffer = new QBuffer(s);
		}

		public override string PayloadToString()
		{
			return $"\t[Pid: {Pid}]";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteU32(m, Pid);
			Buffer.ToBuffer(m);
			return m.ToArray();
		}

		public override string ToString()
		{
			return "[SetPresence Request]";
		}
	}
}
