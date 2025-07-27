using System.IO;
using System.Text;

namespace QuazalWV
{ 
	public class RMCPacketRequestGameSessionService_SearchSessions : RMCPRequest
	{
		public GameSessionQuery Query { get; set; }

		public RMCPacketRequestGameSessionService_SearchSessions(Stream s)
		{
			Query = new GameSessionQuery(s);
		}

		public override string ToString()
		{
			return "[SearchSessions Request]";
		}

		public override string PayloadToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(Query.ToString());
			return sb.ToString();
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Query.ToBuffer(m);
			return m.ToArray();
		}
	}
}
