using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
	public class RMCPacketResponseGameSessionService_SearchSessions : RMCPResponse
	{
		public List<GameSessionSearchResult> Results { get; set; }

		public RMCPacketResponseGameSessionService_SearchSessions(GameSessionQuery query)
		{
			Results = new List<GameSessionSearchResult>();
			foreach (var ses in Global.Sessions)
			{
				if (ses.CheckQuery(query))
				{
					var result = new GameSessionSearchResult
					{
						Key = ses.Key,
						HostPid = ses.HostPid,
						HostUrls = ses.HostUrls,
						Attributes = ses.GameSession.Attributes
					};
					Results.Add(result);
				}		
			}
		}

		public override string PayloadToString()
		{
			return "";
		}

		public override byte[] ToBuffer()
		{
			MemoryStream m = new MemoryStream();
			Helper.WriteU32(m, (uint)Results.Count);
			foreach (var result in Results)
				result.ToBuffer(m);
			return m.ToArray();
		}

		public override string ToString()
		{
			return "[SearchSessions Response]";
		}
	}
}
