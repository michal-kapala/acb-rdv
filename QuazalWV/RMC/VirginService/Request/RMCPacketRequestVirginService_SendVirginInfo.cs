using System.IO;

namespace QuazalWV
{
    public class RMCPacketRequestVirginService_SendVirginInfo : RMCPRequest
    {
        public VirginInfo Info { get; set; }

        public RMCPacketRequestVirginService_SendVirginInfo(Stream s)
        {
			Info = new VirginInfo(s);
        }

		public override string PayloadToString()
		{
			return Info.ToString();
		}

		public override string ToString()
        {
            return "[SendVirginInfo Request] ";
        }
        
        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Info.ToBuffer(m);
            return m.ToArray();
        }
    }
}