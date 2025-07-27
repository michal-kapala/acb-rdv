using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class RMCPacketRequestGameSessionService_AddParticipants : RMCPRequest
	{
		public GameSessionKey Key { get; set; }
		public List<uint> PublicPids { get; set; }
		public List<uint> PrivatePids { get; set; }

		public RMCPacketRequestGameSessionService_AddParticipants(Stream s)
		{
			Key = new GameSessionKey(s);

			PublicPids = new List<uint>();
			uint count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				PublicPids.Add(Helper.ReadU32(s));

			PrivatePids = new List<uint>();
			count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				PrivatePids.Add(Helper.ReadU32(s));
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();

			return m.ToArray();
		}

		public override string ToString()
		{
			return "[AddParticipants Request]";
		}
	}
}
