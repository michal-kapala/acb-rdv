using System.IO;

namespace QuazalWV
{
    public enum PlayerStatValue
    {
        ExperiencePointsX1AndTimeX2 = 1,
        AllTimeScoreX2 = 2,
        Unk3 = 3,
        BestSessionScore = 4,
        SessionsPlayedX2 = 5,
        CurrentTemplarScore = 6,
        Unk7 = 7,
        Unk8 = 8,
        SessionsWonX2 = 9,
        AllTimeWonPlayedRatioDiv50 = 10,
        KillScoreX2 = 11,
        AllTimeKillDeathRatioDiv50 = 12,
        AllTimeKillsX2 = 13,
        AllTimeDeathsX2 = 14,
        BestKillDeathRatioDiv100 = 15,
        AverageKillsPerSessionX2 = 16,
        AllTimeStunsX2 = 17,
        AllTimeStunsInARowX2 = 18,
        MaxStunsInARow = 19,
        MaxKillStreak = 20,
        Unk21 = 21,
        Unk22 = 22,
        AllTimeChasesAndEscapesWonX2 = 23,
        AllTimeChasesAndEscapesTriggeredX2 = 24,
        Unk25 = 25,
        EscapesWonX2 = 26,
        EscapesTriggeredX2 = 27,
        Unk28 = 28
    }

    public class PlayerStatisticValues : IData
	{
		public uint UnkInt { get; set; }
		public ulong UnkLong1 { get; set; }
		public ulong UnkLong2 { get; set; }
		public string UnkStr { get; set; }
		public byte UnkByte { get; set; }

		public PlayerStatisticValues(uint score, uint long1, uint long2, string unkStr, byte unkBytes)
		{
			UnkInt = score;
			UnkLong1 = long1;
			UnkLong2 = long2;
			UnkStr = unkStr;
			UnkByte = unkBytes;
		}

		public PlayerStatisticValues(Stream s)
		{
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			UnkInt = Helper.ReadU32(s);
			UnkLong1 = Helper.ReadU64(s);
			UnkLong2 = Helper.ReadU64(s);
			UnkStr = Helper.ReadString(s);
			UnkByte = Helper.ReadU8(s);
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, UnkInt);
			Helper.WriteU64(s, UnkLong1);
			Helper.WriteU64(s, UnkLong2);
			Helper.WriteString(s, UnkStr);
			Helper.WriteU8(s, UnkByte);
		}

        public override string ToString()
        {
            return $"UnkInt: {UnkInt}, UnkLong1: {UnkLong1}, UnkLong2: {UnkLong2}, UnkStr: \"{UnkStr}\", UnkByte: {UnkByte}";
        }

    }
}
