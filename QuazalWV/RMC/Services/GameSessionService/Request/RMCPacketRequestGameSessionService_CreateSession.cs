using System.IO;
using System.Text;

namespace QuazalWV
{
	public class RMCPacketRequestGameSessionService_CreateSession : RMCPRequest
	{
		public GameSession Session { get; set; }

		public RMCPacketRequestGameSessionService_CreateSession(Stream s)
		{
			Session = new GameSession(s);
			// add missing props as requested by SearchSession
			var prop = Session.Attributes.Find(p => p.Id == 0x69);
			if (prop == null)
				Session.Attributes.Add(new Property { Id = 0x69, Value = 0 }); // Min nb players?
			prop = Session.Attributes.Find(p => p.Id == 0x6A);
			if (prop == null)
				Session.Attributes.Add(new Property { Id = 0x6A, Value = 6 }); // Max nb players?
		}

		public override string PayloadToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach(var prop in Session.Attributes)
				sb.AppendLine(prop.ToString());
			return sb.ToString();
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Session.ToBuffer(m);
			return m.ToArray();
		}

		public override string ToString()
		{
			return "[CreateSession Request]";
		}
	}
}
