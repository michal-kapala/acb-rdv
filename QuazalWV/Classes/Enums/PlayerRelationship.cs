namespace QuazalWV
{
    public enum PlayerRelationship
    {
        /// <summary>
        /// Incoming invitation request.
        /// </summary>
        PendingIn = 0,
        /// <summary>
        /// Outgoing invitation request.
        /// </summary>
        PendingOut = 1,
        /// <summary>
        /// One of the players has blocked the other.
        /// </summary>
        Blocked = 2,
        /// <summary>
        /// The players are friends.
        /// </summary>
        Friend = 3,
    }
}
