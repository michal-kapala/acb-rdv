using System;
using System.Drawing;
using System.IO;

namespace QuazalWV
{
    public class PlayerStatisticInfo : IData
    {
        const uint MaxStatId = 28;
        public byte Id { get; set; }
        public PlayerStatisticValues Value { get; set; }
        public byte UnkByte { get; set; }

        public PlayerStatisticInfo(byte id, byte statid)
        {
            Id = id;
            if (id > MaxStatId)
            {
                Log.WriteRmcLine(1, $"Unknown stat ID {id}", RMCP.PROTOCOL.HermesPlayerStats, LogSource.RMC, Color.Red);
                Value = new PlayerStatisticValues(1, 2, 3, "", 1);
                UnkByte = 1;
            }
            else
            {
                switch ((PlayerStatValue)id)
                {
                    case PlayerStatValue.ExperiencePointsX1AndTimeX2:
                        if (statid == 1)
                            // XP
                            Value = new PlayerStatisticValues(650000, 10, 20, "XP", 1);
                        else
                            // Time
                            Value = new PlayerStatisticValues(0, 10, 20, "Time", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeScoreX2:
                        Value = new PlayerStatisticValues(0, 30, 50, "AllTimeScoreX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.BestSessionScore:
                        Value = new PlayerStatisticValues(0, 30, 50, "BestSessionScore", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.SessionsPlayedX2:
                        Value = new PlayerStatisticValues(0, 111, 7111, "SessionsPlayedX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.SessionsWonX2:
                        Value = new PlayerStatisticValues(0, 111, 7111, "SessionsWonX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeKillsX2:
                        Value = new PlayerStatisticValues(0, 111, 7111, "AllTimeKillsX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeDeathsX2:
                        Value = new PlayerStatisticValues(0, 111, 7111, "AllTimeDeathsX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.BestKillDeathRatioDiv100:
                        Value = new PlayerStatisticValues(0, 111, 7111, "BestKillDeathRatioDiv100", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AverageKillsPerSessionX2:
                        Value = new PlayerStatisticValues(0, 111, 7111, "AverageKillsPerSessionX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeStunsX2:
                        Value = new PlayerStatisticValues(0, 111, 7111, "AllTimeStunsX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeStunsInARowX2:
                        Value = new PlayerStatisticValues(0, 111, 7111, "AllTimeStunsInARowX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.MaxStunsInARow:
                        Value = new PlayerStatisticValues(0, 111, 7111, "MaxStunsInARow", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.MaxKillStreak:
                        Value = new PlayerStatisticValues(0, 111, 7111, "MaxKillStreak", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeChasesAndEscapesWonX2:
                        Value = new PlayerStatisticValues(0, 111, 7111, "AllTimeChasesAndEscapesWonX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeChasesAndEscapesTriggeredX2:
                        Value = new PlayerStatisticValues(0, 111, 7111, "AllTimeChasesAndEscapesTriggeredX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.EscapesWonX2:
                        Value = new PlayerStatisticValues(0, 10, 90, "EscapesWonX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.EscapesTriggeredX2:
                        Value = new PlayerStatisticValues(0, 111, 7111, "EscapesTriggeredX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.CurrentTemplarScore:
                        Value = new PlayerStatisticValues(0, 48, 26, "CurrentTemplarScore", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.KillScoreX2:
                        Value = new PlayerStatisticValues(0, 48, 26, "KillScoreX2", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeWonPlayedRatioDiv50:
                        Value = new PlayerStatisticValues(0, 48, 26, "AllTimeWonPlayedRatioDiv50", 1);
                        UnkByte = 1;
                        break;
                    case PlayerStatValue.AllTimeKillDeathRatioDiv50:
                        Value = new PlayerStatisticValues(0, 48, 26, "AllTimeKillDeathRatioDiv50", 1);
                        UnkByte = 1;
                        break;
                    default:
                        Value = new PlayerStatisticValues(0, 48, 26, "", 0);
                        UnkByte = 1;
                        break;
                }
            }
        }

        public PlayerStatisticInfo(Stream s)
        {
            FromStream(s);
        }

        public void FromStream(Stream s)
        {
            Id = Helper.ReadU8(s);
            Value = new PlayerStatisticValues(s);
            UnkByte = Helper.ReadU8(s);
        }

        public void ToBuffer(Stream s)
        {
            Helper.WriteU8(s, Id);
            Value.ToBuffer(s);
            Helper.WriteU8(s, UnkByte);
        }
        public override string ToString()
        {
            string statName = Enum.GetName(typeof(PlayerStatValue), Id);
            return $"StatId: {Id}, StatName: {statName}, Value: {Value}, UnkByte: {UnkByte}";
        }
    }
}
