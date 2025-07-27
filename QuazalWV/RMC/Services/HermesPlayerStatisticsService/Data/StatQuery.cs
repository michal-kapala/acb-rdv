using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class StatQuery : IData
	{
		public uint StatId { get; set; }
		public List<uint> InfoIds { get; set; }

		public StatQuery(Stream s)
		{
			InfoIds = new List<uint>();
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			StatId = Helper.ReadU32(s);
			uint count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				InfoIds.Add(Helper.ReadU32(s));
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, StatId);
			Helper.WriteU32(s, (uint)InfoIds.Count);
			foreach (uint infoId in InfoIds)
				Helper.WriteU32(s, infoId);
		}
	}
}
