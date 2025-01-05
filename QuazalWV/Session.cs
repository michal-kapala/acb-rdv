using System.Collections.Generic;

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
			GameSession = ses;
			Key = new GameSessionKey
			{
				TypeId = ses.TypeId,
				SessionId = sesId
			};
			PublicPids = new List<uint>();
			PrivatePids = new List<uint>();
			HostPid = host.PID;
			HostUrls = host.Urls;
		}

		public void AddParticipants(List<uint> publicPids, List<uint> privatePids)
		{
			PublicPids.AddRange(publicPids);
			PrivatePids.AddRange(privatePids);
		}

		public bool CheckQuery(GameSessionQuery query)
		{
			foreach (var param in query.Params)
			{
				var attr = GameSession.Attributes.Find(a => a.Id == param.Id);
				if (attr == null || attr.Value != param.Value)
					return false;
			}
			return true;
		}
	}
}
