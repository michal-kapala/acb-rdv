using System.IO;

namespace QuazalWV
{
	public class PlayerStatisticInfoExt : IData
	{
		public byte Id { get; set; }
		public PlayerStatisticValues Value1 { get; set; }
		public byte UnkByte { get; set; }
		public PlayerStatisticValues Value2 { get; set; }
		public PlayerStatisticValues Value3 { get; set; }

		public PlayerStatisticInfoExt(byte id)
		{
			Id = id;
			Value1 = new PlayerStatisticValues();
			UnkByte = 0;
			Value2 = new PlayerStatisticValues();
			Value3 = new PlayerStatisticValues();
		}

		public PlayerStatisticInfoExt(Stream s)
		{
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			Id = Helper.ReadU8(s);
			Value1 = new PlayerStatisticValues(s);
			UnkByte = Helper.ReadU8(s);
			Value2 = new PlayerStatisticValues(s);
			Value3 = new PlayerStatisticValues(s);
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU8(s, Id);
			Value1.ToBuffer(s);
			Helper.WriteU8(s, UnkByte);
			Value2.ToBuffer(s);
			Value3.ToBuffer(s);
		}
	}
}
