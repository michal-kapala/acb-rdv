using System.Drawing;
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
        Unk6 = 6,
        Unk7 = 7,
        Unk8 = 8,
        SessionsWonX2 = 9,
        Unk10 = 10,
        KillScoreX2 = 11,
        Unk12 = 12,
        AllTimeKillsX2 = 13,
        AllTimeDeathsX2 = 14,
        BestKillDeathRatioDiv100 = 15,
        AverageKillsPerSessionX2 = 16,
        AllTimeStuntsX2 = 17,
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

    public class PlayerStatisticInfoExt : IData
    {
        const uint MaxStatId = 28;
        public byte Id { get; set; }
        public PlayerStatisticValues Value1 { get; set; }
        public byte UnkByte { get; set; }
        public PlayerStatisticValues Value2 { get; set; }
        public PlayerStatisticValues Value3 { get; set; }
        
        public PlayerStatisticInfoExt(byte id, byte statid)
        {
            Id = id;
            if (id > MaxStatId)
            {
                Log.WriteRmcLine(1, $"Unknown stat ID {id}", RMCP.PROTOCOL.HermesPlayerStats, LogSource.RMC, Color.Red);
                Value1 = new PlayerStatisticValues(1, 2, 3, "", 1);
                UnkByte = 1;
                Value2 = new PlayerStatisticValues(1, 2, 3, "", 1);
                Value3 = new PlayerStatisticValues(1, 2, 3, "", 1);
            }
            else
            {
                switch ((PlayerStatValue)id)
                {
                    case PlayerStatValue.ExperiencePointsX1AndTimeX2:
                        if (statid == 1 )
                            // XP
                            Value1 = new PlayerStatisticValues(650000, 10, 20, "", 1);
                        else
                            // Time
                            Value1 = new PlayerStatisticValues(300, 10, 20, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeScoreX2:
                        Value1 = new PlayerStatisticValues(9990, 30, 50, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.BestSessionScore:
                        Value1 = new PlayerStatisticValues(1000, 30, 50, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.SessionsPlayedX2:
                        Value1 = new PlayerStatisticValues(100, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.SessionsWonX2:
                        Value1 = new PlayerStatisticValues(300, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeKillsX2:
                        Value1 = new PlayerStatisticValues(4, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeDeathsX2:
                        Value1 = new PlayerStatisticValues(2, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.BestKillDeathRatioDiv100:
                        Value1 = new PlayerStatisticValues(212, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AverageKillsPerSessionX2:
                        Value1 = new PlayerStatisticValues(200, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeStuntsX2:
                        Value1 = new PlayerStatisticValues(222, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeStunsInARowX2:
                        Value1 = new PlayerStatisticValues(667, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.MaxStunsInARow:
                        Value1 = new PlayerStatisticValues(668, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.MaxKillStreak:
                        Value1 = new PlayerStatisticValues(669, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeChasesAndEscapesWonX2:
                        Value1 = new PlayerStatisticValues(322, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeChasesAndEscapesTriggeredX2:
                        Value1 = new PlayerStatisticValues(333, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.EscapesWonX2:
                        Value1 = new PlayerStatisticValues(24, 10, 90, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.EscapesTriggeredX2:
                        Value1 = new PlayerStatisticValues(23, 111, 7111, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.Unk6:
                        Value1 = new PlayerStatisticValues(26, 48, 26, "", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.KillScoreX2:
                        Value1 = new PlayerStatisticValues(10, 48, 26, "", 1);
                        UnkByte = 1;
                        break;
                    default:
                        Value1 = new PlayerStatisticValues(26, 48, 26, "", 0);
                        UnkByte = 1;
                        break;
                }
                Value2 = new PlayerStatisticValues(522, 722, 28, "", 1);
                Value3 = new PlayerStatisticValues(622, 22, 25, "", 1);
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
