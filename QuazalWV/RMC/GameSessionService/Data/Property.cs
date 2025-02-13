using System;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
	public class Property : IData
	{
		public enum MatchmakingParam
		{
			Unk0 = 0,
			MaxPublicSlots = 3,
			MaxPrivateSlots = 4,
			CurrentPublicSlots  = 5,
			CurrentPrivateSlots = 6,
			Unk7 = 7,
			MaxPublicSlots2 = 0x32,
			MaxPrivateSlots2 = 0x33,
			Unk64 = 0x64,
			Unk65 = 0x65,
			Unk66 = 0x66,
			Unk67 = 0x67,
			Unk68 = 0x68,
			Unk69 = 0x69,
			Unk6A = 0x6A,
			Unk6B = 0x6B,
			Unk6C = 0x6C,
			Unk6D = 0x6D,
			Unk6E = 0x6E,
			Accessibility = 0x6F,
			Unk70 = 0x70,
			Unk71 = 0x71,
			Unk72 = 0x72,
			Unk73 = 0x73
		}

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

		public override string ToString()
		{
			string name = Enum.GetName(typeof(MatchmakingParam), Id);
			if (name == null)
				Log.WriteLine(1, $"[RMC] Param name not found for id={Id}", Color.Red);
			return $"[{name:2X}: {Value}]";
		}
	}
}
