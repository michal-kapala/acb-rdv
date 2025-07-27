namespace QuazalWV
{
    /// <summary>
    /// Notification category.
    /// <see cref="https://github.com/ReHamster/AlcatrazServer/blob/d6f41b4b0832f6e7b83e9d3e26a6d15b7c468616/RDVServices/DDL/Models/NotificationEventManager/NotificationEvent.cs#L9"/>
    /// </summary>
    public enum NotificationEventType
    {
        Friends = 1,
        /// <summary>
        /// Unsupported by ACB.
        /// </summary>
        SessionLaunch = 2,
        Participation = 3,
        /// <summary>
        /// Unsupported by ACB.
        /// </summary>
        OwnershipChange = 4,
        FriendStatusChange = 5,
        ForceDisconnect = 6,
        GameSession = 7,
        FirstUserNotification = 1000,
        // Hermes::CustomNotificationEvents
        SwissRoundTournament = 1001,
        MetaSession = 1002,
        Clan = 1003,
        /// <summary>
        /// Unsupported by ACB.
        /// </summary>
        HermesPartySession = 1004,
        /// <summary>
        /// Unsupported by ACB.
        /// </summary>
        PartyProbeMatchmaking = 1005,
        /// <summary>
        /// Unsupported by ACB.
        /// </summary>
        PartyJoinMatchmaking = 1006,
        /// <summary>
        /// Unsupported by ACB.
        /// </summary>
        Statistics = 1008,
    }
}
