using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class GameSessionUpdate : IData
	{
		public GameSessionKey Key { get; set; }
		public List<Property> Attributes { get; set; }

		public GameSessionUpdate()
		{
			Key = new GameSessionKey();
			Attributes = new List<Property>();
		}

		public GameSessionUpdate(Stream s)
		{
			Attributes = new List<Property>();
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			Key = new GameSessionKey(s);
			uint count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				Attributes.Add(new Property(s));
		}

		public void ToBuffer(Stream s)
		{
			Key.ToBuffer(s);
			Helper.WriteU32(s, (uint)Attributes.Count);
			foreach (Property p in Attributes)
				p.ToBuffer(s);
		}
	}
}
