
using System.IO;

public enum PlayerStatValue
{
    ExperiencePointsX1AndTimeX2=1,
    AllTimeScoreX2=2,
    UnkValue=3,
    BestSessionScore=4,
    SessionsPlayedX2=5,
    UnkValue2=6,
    UnkValue3=7,
    UnkValue4=8,
    SessionsWonX2=9,
    UnkValue5=10,
    KillScoreX2= 11,
    UnkValue7=12,
    AllTimeKillsX2=13,
    AllTimeDeathsX2=14,
    BestKillDeathRatioDiv100 = 15,
    AverageKillsPerSessionX2=16,
    AllTimeStuntsX2 = 17,
    AllTimeStunsInARowx2=18,
    MaxStunsInARow=19,
    MaxKillStreak=20,
    UnkValue9=21,
    UnkValue10=22,
    AllTimeChasesAndEscapesWonX2 = 23,
    AllTimeChasesAndEscapesTriggeredX2 = 24,
    UnkValue11=25,
    EscapesWonX2 =26,
    EscapesTriggeredX2=27,
    UnkValue12 = 28
}
namespace QuazalWV
{
	public class PlayerStatisticInfoExt : IData
	{
        const int KnownIdValues = 28;
        public byte Id { get; set; }
		public PlayerStatisticValues Value1 { get; set; }
		public byte UnkByte { get; set; }
		public PlayerStatisticValues Value2 { get; set; }
		public PlayerStatisticValues Value3 { get; set; }

		public PlayerStatisticInfoExt(byte id,byte statid)
		{
            
            Id = id;
            if (id > KnownIdValues)
            {
                Log.WriteLine(1, "Id value is over the known value note it down add it to the enum and raise the known id value count " + id);
                Value1 = new PlayerStatisticValues(1, 2, 3, "overpass1", 1);
                UnkByte = 1;
                Value2 = new PlayerStatisticValues(1, 2, 3, "overpass2", 1);
                Value3 = new PlayerStatisticValues(1, 2, 3, "overpass3", 1);
            }
            else
            {
                switch ((PlayerStatValue)id)
                {
                    case PlayerStatValue.ExperiencePointsX1AndTimeX2:
                        Log.WriteLine(1, "Id" + id);
                        if (statid == 1 )
                        {
                            Value1 = new PlayerStatisticValues(2000, 10, 20, "ExperiencePoints", 1);
                        }
                        else
                        {
                            Value1 = new PlayerStatisticValues(300, 10, 20, "Time", 1);
                        }
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeScoreX2:
                        Value1 = new PlayerStatisticValues(9990, 30, 50, "AllTimeScoreX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.BestSessionScore:
                        Value1 = new PlayerStatisticValues(1000, 30, 50, "BestSessionScoreX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.SessionsPlayedX2:
                        Value1 = new PlayerStatisticValues(100, 111, 7111, "SessionsPlayedX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.SessionsWonX2:
                        Value1 = new PlayerStatisticValues(300, 111, 7111, "SessionsWonX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeKillsX2:
                        Value1 = new PlayerStatisticValues(4, 111, 7111, "AllTimeKillsX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeDeathsX2:
                        Value1 = new PlayerStatisticValues(2, 111, 7111, "AllTimeDeathsX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.BestKillDeathRatioDiv100:
                        Value1 = new PlayerStatisticValues(212, 111, 7111, "BestKillDeathRatioDiv100", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AverageKillsPerSessionX2:
                        Value1 = new PlayerStatisticValues(200, 111, 7111, "AverageKillsPerSessionX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeStuntsX2:
                        Value1 = new PlayerStatisticValues(222, 111, 7111, "AllTimeStuntsX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeStunsInARowx2:
                        Value1 = new PlayerStatisticValues(667, 111, 7111, "AllTimeStunsInARowx2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.MaxStunsInARow:
                        Value1 = new PlayerStatisticValues(668, 111, 7111, "MaxStunsInARow", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.MaxKillStreak:
                        Value1 = new PlayerStatisticValues(669, 111, 7111, "MaxKillStreak", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeChasesAndEscapesWonX2:
                        Value1 = new PlayerStatisticValues(322, 111, 7111, "AllTimeChasesAndEscapesTriggeredX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeChasesAndEscapesTriggeredX2:
                        Value1 = new PlayerStatisticValues(333, 111, 7111, "AllTimeChasesAndEscapesWonX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.EscapesWonX2:
                        Value1 = new PlayerStatisticValues(24, 10, 90, "EscapesWonX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.EscapesTriggeredX2:
                        Value1 = new PlayerStatisticValues(23, 111, 7111, "EscapesTriggeredX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.UnkValue2:
                        Value1 = new PlayerStatisticValues(26, 48, 26, "Unkown Int", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.KillScoreX2:
                        Value1 = new PlayerStatisticValues(10, 48, 26, "Unkown Int", 1);
                        UnkByte = 1;
                        break;
                    default:
                        Value1 = new PlayerStatisticValues(26, 48, 26, "Unkown Int", 0);
                        UnkByte = 1;
                        break;
                }
            Value2 = new PlayerStatisticValues(522, 722, 28, "NotUsed", 1);
            Value3 = new PlayerStatisticValues(622, 22,25, "NotUsed", 1);
            }
            


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
