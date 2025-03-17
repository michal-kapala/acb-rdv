using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

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
					var host = Global.Clients.Find(c => c.PID == ses.HostPid);
					if (host == null)
						Log.WriteLine(1, $"[RMC GameSession] Error: host {ses.HostPid} not found for SearchSessions result", Color.Red);

					var result = new GameSessionSearchResult
					{
						Key = ses.Key,
						HostPid = ses.HostPid,
						HostUrls = host != null ? host.RegisteredUrls : new List<StationUrl>(),
						Attributes = ses.FilterAttributes()
					};
					Log.WriteLine(1, $"[RMC GameSession] GameSessionSearchResult: {result}");
					Results.Add(result);
				}		
			}
		}

		public override string PayloadToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"[Results: {Results.Count}]");
			foreach (var result in Results)
				sb.AppendLine(result.ToString());
			return sb.ToString();
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
