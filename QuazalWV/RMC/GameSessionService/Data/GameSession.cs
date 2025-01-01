using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class GameSession : IData
	{
		public uint TypeId { get; set; }
		public List<Property> Attributes { get; set; }

		public GameSession()
		{
			Attributes = new List<Property>();
		}

		public GameSession(Stream s)
		{
			Attributes = new List<Property>();
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			TypeId = Helper.ReadU32(s);
			uint count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				Attributes.Add(new Property(s));
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, TypeId);
			Helper.WriteU32(s, (uint)Attributes.Count);
			foreach (Property p in Attributes)
				p.ToBuffer(s);
		}
	}
}
