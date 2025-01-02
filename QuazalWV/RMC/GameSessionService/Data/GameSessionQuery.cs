using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class GameSessionQuery : IData
	{
		public uint TypeId { get; set; }
		public uint QueryId { get; set; }
		public List<Property> Params { get; set; }

		public GameSessionQuery()
		{

		}

		public GameSessionQuery(Stream s)
		{
			Params = new List<Property>();
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			TypeId = Helper.ReadU32(s);
			QueryId = Helper.ReadU32(s);
			uint count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				Params.Add(new Property(s));
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, TypeId);
			Helper.WriteU32(s, QueryId);
			Helper.WriteU32(s, (uint)Params.Count);
			foreach (Property prop in Params)
				prop.ToBuffer(s);
		}
	}
}
