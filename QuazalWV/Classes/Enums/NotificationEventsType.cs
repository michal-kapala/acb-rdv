using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuazalWV.Classes.Enums
{
    public enum NotificationEventsType
    {
        FriendEvent = 1,
        SessionLaunched = 2,
        ParticipationEvent = 3,
        OwnershipChangeEvent = 4,
        FriendStatusChangeEvent = 5,
        ForceDisconnectEvent = 6,
        GameSessionEvent = 7,
        FirstUserNotification = 1000,
        // Hermes::CustomNotificationEvents start
        SwissRoundTournament = 1001,
        MetaSession = 1002,
        Clans = 1003,
        HermesPartySession = 1004,
        PartyProbeMatchmaking = 1005,
        PartyJoinMatchmaking = 1006,
        Statistics = 1008,
    };
}
