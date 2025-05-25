using System.IO;
using System.Text;

namespace QuazalWV
{
	public class VirginInfo : IData
	{
		public uint Pid { get; set; }
		public ulong SessionID { get; set; }
		public QDateTime Begin {  get; set; }
		public QDateTime End { get; set; }
		public bool Host { get; set; }
		public bool HasQuit { get; set; }
		public uint UniquePlayerMax { get; set; }
		public byte GameMode { get; set; }
		public byte Platform { get; set; }
		public uint Score { get; set; }
		public uint KillNumber { get; set; }

		public VirginInfo() { }

		public VirginInfo(Stream s)
		{
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			Pid = Helper.ReadU32(s);
			SessionID = Helper.ReadU64(s);
			Begin = new QDateTime(s);
			End = new QDateTime(s);
			Host = Helper.ReadBool(s);
			HasQuit = Helper.ReadBool(s);
			UniquePlayerMax = Helper.ReadU32(s);
			GameMode = Helper.ReadU8(s);
			Platform = Helper.ReadU8(s);
			Score = Helper.ReadU32(s);
			KillNumber = Helper.ReadU32(s);
		}

		public void ToBuffer(Stream s)
		{
			Helper.WriteU32(s, Pid);
			Helper.WriteU64(s, SessionID);
			Begin.ToBuffer(s);
			End.ToBuffer(s);
			Helper.WriteBool(s, Host);
			Helper.WriteBool(s, HasQuit);
			Helper.WriteU32(s, UniquePlayerMax);
			Helper.WriteU8(s, GameMode);
			Helper.WriteU8(s, Platform);
			Helper.WriteU32(s, Score);
			Helper.WriteU32(s, KillNumber);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"\t[UserDBPid: {Pid}]");
			sb.AppendLine($"\t[Session ID: {SessionID}]");
			sb.AppendLine($"\t[Date begin: {Begin.Time}]");
			sb.AppendLine($"\t[Date end: {End.Time}]");
			string host = Host ? "yes" : "no";
			sb.AppendLine($"\t[Host: {host}]");
			string quit = HasQuit ? "yes" : "no";
			sb.AppendLine($"\t[Has quit: {quit}]");
			sb.AppendLine($"\t[Max unique players: {UniquePlayerMax}]");
			sb.AppendLine($"\t[Game mode: {GameMode}]");
			sb.AppendLine($"\t[Platform: {Platform}]");
			sb.AppendLine($"\t[Score: {Score}]");
			sb.AppendLine($"\t[Kills: {KillNumber}]");
			return sb.ToString();
		}
	}
}
