using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.IO;

namespace QuazalWV
{
    internal class RMCPacketResponseGameSessionService_GetSession : RMCPResponse
    {
        public GameSessionSearchResult Result;

        public RMCPacketResponseGameSessionService_GetSession(GameSessionKey key, ClientInfo client)
        {
            Result = new GameSessionSearchResult();
            Log.WriteLine(1, $"{Global.Sessions.Count}");

            foreach (var sessionIter in Global.Sessions)
            {
                Log.WriteLine(1, $"{sessionIter}");
                if (sessionIter.Key.SessionId==key.SessionId && sessionIter.Key.TypeId == key.TypeId)
                {
                    var sessionHost=Global.Clients.FirstOrDefault(p => p.ServerIncrementedGeneratedPID == sessionIter.HostPid);
                    if (sessionHost == null)
                    {
                        Log.WriteLine(1, "session without a host and with an invite -> please investigate", Color.Red);
                    }

                    Result = new GameSessionSearchResult
                    {
                        Key = sessionIter.Key,
                        HostPid = sessionIter.HostPid,
                        HostUrls = sessionHost != null ? sessionHost.RegisteredUrls : new List<StationUrl>(),
                        Attributes = sessionIter.FilterAttributes()
                    };
                    Log.WriteLine(1, $"GetSession found {Result}", Color.Red);
                    return;
                }
            }
            Log.WriteLine(1, $"No sessions with that key");
        }
        // uint src, uint type, uint subtype, uint parameter1, uint parameter2, uint parameter3, string parameterString)
        public override string ToString()
        {
            return "[GameSessionService_GetSession Request]";
        }

        public override string PayloadToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Result.ToString());
            return sb.ToString();
        }

        public override byte[] ToBuffer()
        {
            MemoryStream m = new MemoryStream();
            Result.ToBuffer(m);
            return m.ToArray();
        }
    }
}