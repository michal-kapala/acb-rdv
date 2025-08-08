using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace QuazalWV
{
    public class Session
    {
        public GameSession GameSession { get; set; }
        public GameSessionKey Key { get; set; }
        public List<uint> PublicPids {  get; set; }
        public List<uint> PrivatePids { get; set; }
        public uint HostPid { get; set; }
        public List<StationUrl> HostUrls { get; set; }
        public bool Migrating { get; set; } = false;

        public Session(uint sesId, GameSession ses, ClientInfo host)
        {
            GameSession = ses;
            Key = new GameSessionKey
            {
                TypeId = ses.TypeId,
                SessionId = sesId
            };
            PublicPids = new List<uint>();
            PrivatePids = new List<uint>();
            HostPid = host.User.Pid;
            foreach (var url in host.Urls)
                Log.WriteLine(2, $"Host URL added: ${url}", LogSource.Session, Color.Green);
            HostUrls = host.Urls;
        }

        public void AddParticipants(List<uint> publicPids, List<uint> privatePids)
        {
            PublicPids.AddRange(publicPids);
            PrivatePids.AddRange(privatePids);
            UpdateCurrentSlots();
        }

        public bool CheckQuery(GameSessionQuery query, ClientInfo client)
        {
            var qMinLevelRange = query.Params.Find(param => param.Id == (uint)SessionParam.MinLevelRange);
            var qMaxLevelRange = query.Params.Find(param => param.Id == (uint)SessionParam.MaxLevelRange);
            var qGameMode = query.Params.Find(param => param.Id == (uint)SessionParam.GameMode);
            var qMaxSlotsTaken = query.Params.Find(param => param.Id == (uint)SessionParam.QueryMaxSlotsTaken);
            var qGameType = query.Params.Find(param => param.Id == (uint)SessionParam.GameType);
            // query integrity check
            if (qMinLevelRange == null || qMaxLevelRange == null || qGameMode == null || qGameType == null)
            {
                Log.WriteLine(1, $"Inconsistent session state (id={Key.SessionId}), failed integrity check", LogSource.Session, Color.Red, client);
                return false;
            }

            // self-hosted
            if (client.User.Pid == HostPid)
            {
                Log.WriteLine(1, $"Ignoring a self-hosted session", LogSource.Session, Color.Gray, client);
                return false;
            }

            // ignore queries with level limits or without slots
            if (qMinLevelRange.Value == qMaxLevelRange.Value || qMaxSlotsTaken == null)
            {
                Log.WriteLine(1, $"Session ignored due to level ranges/lack of slots", LogSource.Session, Color.Gray, client);
                return false;
            }

            var gameMode = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameMode);
            var gameType = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameType);

            if (gameMode == null || gameType == null)
            {
                Log.WriteLine(1, $"Inconsistent session state (id={Key.SessionId}), missing game mode or type", LogSource.Session, Color.Red, client);
                return false;
            }

            // game mode/type mismatch
            if (gameMode.Value != qGameMode.Value || gameType.Value != qGameType.Value)
            {
                Log.WriteLine(1, $"Session ignored due to game mode/type mismatch", LogSource.Session, Color.Gray, client);
                return false;
            }

            uint slotsParam = gameType.Value == (uint)GameType.PRIVATE ? (uint)SessionParam.CurrentPrivateSlots : (uint)SessionParam.CurrentPublicSlots;
            var currentSlots = GameSession.Attributes.Find(param => param.Id == slotsParam);
            if (currentSlots == null)
            {
                Log.WriteLine(1, $"Inconsistent session state (id={Key.SessionId}), missing current slots", LogSource.Session, Color.Red, client);
                return false;
            }
            
            if (qMaxSlotsTaken == null)
            {
                Log.WriteLine(1, $"Session query missing QueryMaxSlotsTaken parameter", LogSource.Session, Color.Red, client);
                return false;
            }

            // too many players
            if (currentSlots.Value > qMaxSlotsTaken.Value)
            {
                Log.WriteLine(1, $"Session ignored due to too many players", LogSource.Session, Color.Gray, client);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Attribute list for SearchSessions response.
        /// </summary>
        /// <returns></returns>
        public List<Property> FilterAttributes()
        {
            var attrs = new List<Property>();
            foreach (var attr in GameSession.Attributes)
            {
                if (attr.Id == (uint)SessionParam.IsPrivate
                    || attr.Id == (uint)SessionParam.MinLevelRange
                    || attr.Id == (uint)SessionParam.MaxLevelRange
                    || attr.Id == (uint)SessionParam.PunkbusterActive)
                    continue;
                attrs.Add(attr);
            }
            return attrs;
        }

        public void RemoveParticipants(List<uint> ToRemove)
        {
            PublicPids.RemoveAll(item => ToRemove.Contains(item));
            PrivatePids.RemoveAll(item => ToRemove.Contains(item));
            UpdateCurrentSlots();			
        }

        public void Leave(ClientInfo client)
        {
            PublicPids.Remove(client.User.Pid);
            PrivatePids.Remove(client.User.Pid);
            UpdateCurrentSlots();
            // host left - assign new host (migrate?)
            if (client.User.Pid == HostPid)
            {
                if (PublicPids.Count > 0)
                    HostPid = PublicPids[0];
                else
                    HostPid = PrivatePids[0];
                // this flow should never happen as host migrations use MigrateSession->RegisterURLs->AbandonSession flow
                Log.WriteLine(1, $"On-leave host migration from {client.User.Pid} to {HostPid}", LogSource.Session, Color.Orange);
                var newHost = Global.Clients.Find(c => client.User.Pid == HostPid);
                if (newHost == null)
                {
                    Log.WriteLine(1, $"On-leave host migration elected non-existent host {HostPid}", LogSource.Session, Color.Red);
                    return;
                }
                HostUrls = newHost.RegisteredUrls;	
            }
        }

        public uint NbParticipants()
        {
            return (uint)(PublicPids.Count + PrivatePids.Count);
        }

        private void UpdateCurrentSlots()
        {
            var currPublicSlots = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.CurrentPublicSlots);
            if (currPublicSlots == null)
            {
                GameSession.Attributes.Add(new Property((uint)SessionParam.CurrentPublicSlots, (uint)PublicPids.Count));
                Log.WriteLine(1, $"Session {Key.SessionId} missing public slots param", LogSource.Session, Color.Red);
            }
            else
                currPublicSlots.Value = (uint)PublicPids.Count;

            var currPrivateSlots = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.CurrentPrivateSlots);
            if (currPrivateSlots == null)
            {
                GameSession.Attributes.Add(new Property((uint)SessionParam.CurrentPrivateSlots, (uint)PrivatePids.Count));
                Log.WriteLine(1, $"Session {Key.SessionId} missing private slots param", LogSource.Session, Color.Red);
            }
            else
                currPrivateSlots.Value = (uint)PrivatePids.Count;
        }

        public Property FindProp(SessionParam id)
        {
            return GameSession.Attributes.Find(prop => prop.Id == (uint)id);
        }

        public bool IsJoinable()
        {
            var isPrivateParam = FindProp(SessionParam.IsPrivate);
            if (isPrivateParam == null)
            {
                Log.WriteLine(1, $"IsPrivate attribute missing in session {Key.SessionId}", LogSource.Session, Color.Red);
                return false;
            }
            var maxSlots = FindProp(isPrivateParam.Value == 0 ? SessionParam.MaxPublicSlots : SessionParam.MaxPrivateSlots);
            if (maxSlots == null)
            {
                Log.WriteLine(1, $"MaxSlots attribute missing in session {Key.SessionId}", LogSource.Session, Color.Red);
                return false;
            }
            var currentSlots = FindProp(isPrivateParam.Value == 0 ? SessionParam.CurrentPublicSlots : SessionParam.CurrentPrivateSlots);
            if (currentSlots == null)
            {
                Log.WriteLine(1, $"CurrentSlots attribute missing in session {Key.SessionId}", LogSource.Session, Color.Red);
                return false;
            }
            return maxSlots.Value > currentSlots.Value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"\t[ID: {Key.SessionId}]");
            sb.AppendLine($"\t[Type: {Key.TypeId}]");
            sb.AppendLine($"\t[Host: {HostPid}]");
            sb.AppendLine($"\t[Public PIDs: ({string.Join(", ", PublicPids)})]");
            sb.AppendLine($"\t[Private PIDs: ({string.Join(", ", PrivatePids)})]");
            sb.AppendLine($"\t[Attributes]");
            foreach (var attr in GameSession.Attributes)
                sb.AppendLine($"\t\t{attr}");
            sb.AppendLine($"\t[Host URLs]");
            foreach (var url in HostUrls)
                sb.AppendLine($"\t\t[{url}]");
            return sb.ToString();
        }
    }
}
