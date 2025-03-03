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
			switch((MatchmakingParam)Id)
			{
				case MatchmakingParam.CxbCrcSum:
					return $"[CXB CRC: 0x{Value:X8}]";
				case MatchmakingParam.MapID:
					return EnumToStr("Map", typeof(Map));
				case MatchmakingParam.HostLevelRange:
					return EnumToStr("Level range", typeof(LevelRange));
				case MatchmakingParam.MinLevelRange:
					return EnumToStr("Min level range", typeof(LevelRange));
				case MatchmakingParam.MaxLevelRange:
					return EnumToStr("Max level range", typeof(LevelRange));
				case MatchmakingParam.GameMode:
					return EnumToStr("Mode", typeof(GameMode));
				case MatchmakingParam.GameType:
					return EnumToStr("Type", typeof(GameType));
				case MatchmakingParam.NatType:
					return EnumToStr("NAT", typeof(NatType));
				default:
					string name = Enum.GetName(typeof(MatchmakingParam), Id);
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
