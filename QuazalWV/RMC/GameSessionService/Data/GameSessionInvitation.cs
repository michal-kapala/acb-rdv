using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QuazalWV
{
	public class GameSessionInvitation : IData
	{
		public GameSessionKey Key { get; set; }
		public List<uint> Recipients { get; set; }
		public string Message { get; set; }

		public GameSessionInvitation()
		{

		}

		public GameSessionInvitation(Stream s)
		{
			Recipients = new List<uint>();
			FromStream(s);
		}
		public override string ToString()
		{
            StringBuilder sb = new StringBuilder();
            sb.Append($"Key of the game Invite : {Key}\n");
            sb.Append($"Recipients : {string.Join(", ", Recipients)} \n");
			sb.Append($"Message: {Message}");
            return sb.ToString();
        }

		public void FromStream(Stream s)
		{
			Key = new GameSessionKey(s);
			uint count = Helper.ReadU32(s);
			for (uint i = 0; i < count; i++)
				Recipients.Add(Helper.ReadU32(s));
			Message = Helper.ReadString(s);
		}

		public void ToBuffer(Stream s)
		{
			Key.ToBuffer(s);
			Helper.WriteU32(s, (uint)Recipients.Count);
			foreach (var recipient in Recipients)
				Helper.WriteU32(s, recipient);
			Helper.WriteString(s, Message);
		}
	}
}
