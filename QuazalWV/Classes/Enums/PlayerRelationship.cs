namespace QuazalWV
{
    /// <summary>
    /// Available game mode IDs included in session data.
    /// </summary>
    public enum PlayerRelationship
    {
        pending = 2,
        friend = 1,
        blocked = 3,
        //Friend_Online=3
        /// 0 - incoming invitation request
        /// 1 - outgoing invitation request
        /// 2 - friend (offline? blocked?)
        /// 3 - friend (online?)
        /// 
        //pending ->2 relationship sense matters
        //friend -> 1
        //blocked ->3
    }
}
