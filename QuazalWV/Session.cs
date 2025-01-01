using System.Collections.Generic;

namespace QuazalWV
{
	public class Session
	{
		public GameSession GameSession { get; set; }
		public GameSessionKey Key { get; set; }
		public List<uint> PublicPids {  get; set; }
		public List<uint> PrivatePids { get; set; }

		public Session(uint sesId, GameSession ses)
		{
			GameSession = ses;
			Key = new GameSessionKey
			{
				TypeId = ses.TypeId,
				SessionId = sesId
			};
			PublicPids = new List<uint>();
			PrivatePids = new List<uint>();
		}

		public void AddParticipants(List<uint> publicPids, List<uint> privatePids)
		{
			PublicPids.AddRange(publicPids);
			PrivatePids.AddRange(privatePids);
		}
	}
}
