namespace QuazalWV
{
	/// <summary>
	/// Type of a player-player relationship.
	/// </summary>
	public enum DbPlayerRelationshipType
	{
		/// <summary>
		/// An on-hold invitation from either player.
		/// </summary>
		Pending = 1,
		/// <summary>
		/// One of the players has blocked the other.
		/// </summary>
		Blocked = 2,
		/// <summary>
		/// The players are friends.
		/// </summary>
		Friends = 3,
	}
}
