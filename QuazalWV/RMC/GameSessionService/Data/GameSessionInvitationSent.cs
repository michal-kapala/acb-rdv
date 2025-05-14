using System.IO;
using System.Text;

namespace QuazalWV
{
	public class GameSessionInvitationSent : IData
	{
		public GameSessionKey Key { get; set; }
		public uint RecipientPid { get; set; }
		public string Message { get; set; }
		public QDateTime CreationTime {  get; set; }

		public GameSessionInvitationSent()
		{

		}

		public GameSessionInvitationSent(Stream s)
		{
			FromStream(s);
		}

		public void FromStream(Stream s)
		{
			Key = new GameSessionKey(s);
			RecipientPid = Helper.ReadU32(s);
			Message = Helper.ReadString(s);
			CreationTime = new QDateTime(s);
		}

		public void ToBuffer(Stream s)
		{
			Key.ToBuffer(s);
			Helper.WriteU32(s, RecipientPid);
			Helper.WriteString(s, Message);
			CreationTime.ToBuffer(s);
		}
		public override string  ToString()
        {
			StringBuilder sb = new StringBuilder();
			sb.Append($"Key of the game Invite : {Key}\n");
			sb.Append($"Recipient pid : {string.Join(", ", RecipientPid)} \n");
			sb.Append($"Message: {Message}");
			return sb.ToString();
		}
	}
}
