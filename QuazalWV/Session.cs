using System;
using System.Collections.Generic;
using System.Drawing;

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

        public ClientInfo client;

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
            client = host;
			HostPid = host.PID;
            foreach (var url in host.Urls)
            {
                Log.WriteLine(1, $"[AddedURL] URL added: ${url}", Color.Red);
            }
            HostUrls = host.Urls;
		}

		public void AddParticipants(List<uint> publicPids, List<uint> privatePids)
		{
			PublicPids.AddRange(publicPids);
			PrivatePids.AddRange(privatePids);

			// update slot params
			var currPublicSlots = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.CurrentPublicSlots);
			currPublicSlots.Value = (uint)PublicPids.Count;
			var currPrivateSlots = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.CurrentPrivateSlots);
			currPrivateSlots.Value = (uint)PrivatePids.Count;
		}

		public bool CheckQuery(GameSessionQuery query)
		{
			var qMinLevelRange = query.Params.Find(param => param.Id == (uint)SessionParam.MinLevelRange);
			var qMaxLevelRange = query.Params.Find(param => param.Id == (uint)SessionParam.MaxLevelRange);
			var qGameMode = query.Params.Find(param => param.Id == (uint)SessionParam.GameMode);
			var qMaxSlotsTaken = query.Params.Find(param => param.Id == (uint)SessionParam.QueryMaxSlotsTaken);
			var qGameType = query.Params.Find(param => param.Id == (uint)SessionParam.GameType);
			// query integrity check
			if (qMinLevelRange == null || qMaxLevelRange == null || qGameMode == null || qGameType == null)
			{
				Log.WriteLine(1, $"[Session] Inconsistent session state (id={Key.SessionId}), failed integrity check", Color.Red);
				return false;
			}

			// ignore queries with level limits or without slots
			if (qMinLevelRange.Value == qMaxLevelRange.Value || qMaxSlotsTaken == null)
			{
				Log.WriteLine(1, $"[Session] Session ignored due to level ranges/lack of slots", Color.Gray);
				return false;
			}

			var gameMode = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameMode);
			var gameType = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.GameType);

			if (gameMode == null || gameType == null)
			{
				Log.WriteLine(1, $"[Session] Inconsistent session state (id={Key.SessionId}), missing game mode or type", Color.Red);
				return false;
			}

			// game mode/type mismatch
			if (gameMode.Value != qGameMode.Value || gameType.Value != qGameType.Value)
			{
				Log.WriteLine(1, $"[Session] Session ignored due to game mode/type mismatch", Color.Gray);
				return false;
			}

			uint slotsParam = gameType.Value == (uint)GameType.PRIVATE ? (uint)SessionParam.CurrentPrivateSlots : (uint)SessionParam.CurrentPublicSlots;
			var currentSlots = GameSession.Attributes.Find(param => param.Id == slotsParam);
			if (currentSlots == null)
			{
				Log.WriteLine(1, $"[Session] Inconsistent session state (id={Key.SessionId}), missing current slots", Color.Red);
				return false;
			}

			// too many players
			if (currentSlots.Value > qMaxSlotsTaken.Value)
			{
				Log.WriteLine(1, $"[Session] Session ignored due to too many players", Color.Gray);
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

        internal void RemoveParticipants(List<uint> publicPids, List<uint> privatePids)
        {
            PublicPids.RemoveAll(item => publicPids.Contains(item));
            PrivatePids.RemoveAll(item => privatePids.Contains(item));

            // update slot params
            var currPublicSlots = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.CurrentPublicSlots);
            currPublicSlots.Value = (uint)PublicPids.Count;
            var currPrivateSlots = GameSession.Attributes.Find(param => param.Id == (uint)SessionParam.CurrentPrivateSlots);
            currPrivateSlots.Value = (uint)PrivatePids.Count;
        }
    }
}
