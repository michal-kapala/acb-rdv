namespace QuazalWV
{
    public enum PlayerRelationship
    {
        /// <summary>
        /// The players are friends.
        /// </summary>
        Friend = 1,
        /// <summary>
        /// Invitation request.
        /// </summary>
        Pending = 2,
        /// <summary>
        /// One of the players has blocked the other.
        /// </summary>
        Blocked = 3,
    }
}
