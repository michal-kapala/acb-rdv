using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace QuazalWV
{
    public class RMCPacketResponseGameSessionService_SearchSessions : RMCPResponse
    {
        public List<GameSessionSearchResult> Results { get; set; }

        public RMCPacketResponseGameSessionService_SearchSessions(GameSessionQuery query, ClientInfo client)
        {
            Results = new List<GameSessionSearchResult>();
            var validSessions = new List<Session>();
            // Filter and clean sessions
            for (int i = Global.Sessions.Count - 1; i >= 0; i--)
            {
                var ses = Global.Sessions[i];

                if (!ses.CheckQuery(query, client))
                    continue;

                var host = Global.Clients.Find(c => c.User.Pid == ses.HostPid);
                if (host == null)
                {
                    Log.WriteRmcLine(1, $"Error: host {ses.HostPid} not found for SearchSessions result — removing invalid session",
                        RMCP.PROTOCOL.GameSession, LogSource.RMC, Color.Red, client);
                    Global.Sessions.RemoveAt(i);
                    continue;
                }
                validSessions.Add(ses);
            }
            // Filter only joinable sessions and prioritize fuller sessions
            var sortedSessions = validSessions
                .Where(s => s.IsJoinable())
                .OrderByDescending(s => s.NbParticipants())
                .ToList();
            foreach (var ses in sortedSessions)
            {
                var host = Global.Clients.Find(c => c.User.Pid == ses.HostPid);
                if (host == null)
                    continue;
                var result = new GameSessionSearchResult
                {
                    Key = ses.Key,
                    HostPid = ses.HostPid,
                    HostUrls = host.RegisteredUrls,
                    Attributes = ses.FilterAttributes()
                };
                Log.WriteRmcLine(1, $"GameSessionSearchResult: {result}",
                    RMCP.PROTOCOL.GameSession, LogSource.RMC, Color.Black, client);
                Results.Add(result);
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
