using System.Collections.Generic;
using System.Drawing;
using System.Security.Policy;

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

		public Session(uint sesId, GameSession ses, ClientInfo host)
		{
            Log.WriteLine(2, $"[Session] Client: ${host.User.Name} created Session: ${sesId}", Color.Green);
            GameSession = ses;
			Key = new GameSessionKey
			{
				TypeId = ses.TypeId,
				SessionId = sesId
			};
			PublicPids = new List<uint>();
			PrivatePids = new List<uint>();
			HostPid = host.PID;
			foreach (var url in host.Urls)
				Log.WriteLine(2, $"[Session] Host URL added: ${url}", Color.Green);
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
				Log.WriteLine(1, $"[Session] Inconsistent session state (id={Key.SessionId}), failed integrity check", Color.Red, client);
				return false;
			}

			// self-hosted
			if (client.PID == HostPid)
			{
				Log.WriteLine(1, $"[Session] Ignoring a self-hosted session", Color.Gray, client);
				return false;
			}

			// ignore queries with level limits or without slots
			if (qMinLevelRange.Value == qMaxLevelRange.Value || qMaxSlotsTaken == null)
			{
				Log.WriteLine(1, $"[Session] Session ignored due to level ranges/lack of slots", Color.Gray, client);
				return false;
			}

			var gameMode = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameMode);
			var gameType = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameType);

			if (gameMode == null || gameType == null)
			{
				Log.WriteLine(1, $"[Session] Inconsistent session state (id={Key.SessionId}), missing game mode or type", Color.Red, client);
				return false;
			}

			// game mode/type mismatch
			if (gameMode.Value != qGameMode.Value || gameType.Value != qGameType.Value)
			{
				Log.WriteLine(1, $"[Session] Session ignored due to game mode/type mismatch", Color.Gray, client);
				return false;
			}

			uint slotsParam = gameType.Value == (uint)GameType.PRIVATE ? (uint)SessionParam.CurrentPrivateSlots : (uint)SessionParam.CurrentPublicSlots;
			var currentSlots = GameSession.Attributes.Find(param => param.Id == slotsParam);
			if (currentSlots == null)
			{
				Log.WriteLine(1, $"[Session] Inconsistent session state (id={Key.SessionId}), missing current slots", Color.Red, client);
				return false;
			}

			// too many players
			if (currentSlots.Value > qMaxSlotsTaken.Value)
			{
				Log.WriteLine(1, $"[Session] Session ignored due to too many players", Color.Gray, client);
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

		public void RemoveParticipants(List<uint> publicPids)
		{
			PublicPids.RemoveAll(item => publicPids.Contains(item));
			UpdateCurrentSlots();
		}

		public void Leave(ClientInfo client)
		{
			PublicPids.Remove(client.PID);
			PrivatePids.Remove(client.PID);
			UpdateCurrentSlots();
			// host left - assign new host (migrate?)
			if (client.PID == HostPid)
			{
				if (PublicPids.Count > 0)
					HostPid = PublicPids[0];
				else
					HostPid = PrivatePids[0];
				Log.WriteLine(1, $"[Session] On-leave host migration from {client.PID} to {HostPid}", Color.Orange);
				var newHost = Global.Clients.Find(c => c.PID == HostPid);
				if (newHost == null)
				{
					Log.WriteLine(1, $"[Session] On-leave host migration elected non-existent host {HostPid}", Color.Red);
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
            Log.WriteLine(1, $"[RMC] GameSession: {GameSession}", Color.HotPink);

            Log.WriteLine(1, $"[RMC] SearchSessions results: {currPublicSlots}",Color.Purple);
            Log.WriteLine(1, $"[RMC] SearchSessions results: {PublicPids.Count}",Color.Red);
			if (currPublicSlots == null)
			{
				
                GameSession.Attributes.Add(new Property((uint)SessionParam.CurrentPublicSlots,(uint) PublicPids.Count));
                Log.WriteLine(1, $"[RMC] -------This should not happen ----- {Key.SessionId}", Color.Purple);
            }
            else
            {
                currPublicSlots.Value = (uint)PublicPids.Count;

            }

            var currPrivateSlots = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.CurrentPrivateSlots);
            if (currPrivateSlots == null)
            {
                GameSession.Attributes.Add(new Property((uint)SessionParam.CurrentPrivateSlots, (uint)PrivatePids.Count));
                Log.WriteLine(1, $"[RMC] -------This should not happen2 ----- {Key.SessionId}", Color.Purple);

            }
            else
            {
                currPrivateSlots.Value = (uint)PrivatePids.Count;

            }
            
		}

    }
}
