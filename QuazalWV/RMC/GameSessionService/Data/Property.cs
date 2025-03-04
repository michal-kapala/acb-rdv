using System;
using System.Drawing;
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

		public override string ToString()
		{
			switch((SessionParam)Id)
			{
				case SessionParam.CxbCrcSum:
					return $"[CXB CRC: 0x{Value:X8}]";
				case SessionParam.MapID:
					return EnumToStr("Map", typeof(Map));
				case SessionParam.HostLevelRange:
					return EnumToStr("Level range", typeof(LevelRange));
				case SessionParam.MinLevelRange:
					return EnumToStr("Min level range", typeof(LevelRange));
				case SessionParam.MaxLevelRange:
					return EnumToStr("Max level range", typeof(LevelRange));
				case SessionParam.GameMode:
					return EnumToStr("Mode", typeof(GameMode));
				case SessionParam.GameType:
					return EnumToStr("Type", typeof(GameType));
				case SessionParam.NatType:
					return EnumToStr("NAT", typeof(NatType));
				default:
					string name = Enum.GetName(typeof(SessionParam), Id);
					if (name == null)
					{
						Log.WriteLine(1, $"[RMC] Param name not found for id={Id}", Color.Red);
						return $"[Unk{Id:X2}: {Value}]";
					}
					return $"[{name}: {Value}]";
			}
		}

		private string EnumToStr(string label, Type type)
		{
			return $"[{label}: {Enum.GetName(type, Value)}]";
		}
	}
}
