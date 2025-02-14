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
			if (Id == (uint)MatchmakingParam.MapID)
				return $"[Map: {Enum.GetName(typeof(Map), Value)}]";

			if (Id == (uint)MatchmakingParam.GameMode)
				return $"[Mode: {Enum.GetName(typeof(GameMode), Value)}]";

			string name = Enum.GetName(typeof(MatchmakingParam), Id);
			if (name == null)
				Log.WriteLine(1, $"[RMC] Param name not found for id={Id}", Color.Red);
			return $"[{name:2X}: {Value}]";
		}
	}
}
