using System.IO;

namespace QuazalWV
{
	public class Property : IData
	{
		public uint Id {  get; set; }
		public uint Value { get; set; }

		public Property()
		{
			
		}

		public Property(Stream s)
		{
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			Id = Helper.ReadU32(s);
			Value = Helper.ReadU32(s);
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, Id);
			Helper.WriteU32(s, Value);
		}
	}
}
