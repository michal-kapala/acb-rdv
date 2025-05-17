using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuazalWV
{
    public static class NotificationManager
    {
        /// <summary>
        /// Sends a friend invite denial notification.
        /// </summary>
        /// <param name="receiverClient"></param>
        /// <param name="senderPid"></param>
        /// <param name="senderName"></param>
        public static void FriendInviteDeclined(ClientInfo receiverClient, uint senderPid, string senderName)
        {
            new NotificationEvent(
                receiverClient,
                0,
                senderPid,
                (uint)NotificationEventType.Friends,
                1,
                senderPid,
                (uint)FriendsNotificationParam2.FriendshipDeclined,
                0,
                senderName
                ).Send();
        }

        /// <summary>
        /// Sends a friend invite acceptance notification.
        /// </summary>
        /// <param name="receiverClient"></param>
        /// <param name="senderPid"></param>
        /// <param name="senderName"></param>
        public static void FriendInviteAccepted(ClientInfo receiverClient, uint senderPid, string senderName)
        {
            new NotificationEvent(
                receiverClient,
                0,
                senderPid,
                (uint)NotificationEventType.Friends,
                1,
                senderPid,
                (uint)FriendsNotificationParam2.FriendshipAccepted,
                0,
                senderName
                ).Send();
        }

        /// <summary>
        /// Sends a friend invite notification.
        /// <param name="receiverClient"></param>
        /// <param name="senderPid"></param>
        /// <param name="senderName"></param>
        /// </summary>
        public static void FriendInviteReceived(ClientInfo receiverClient, uint senderPid, string senderName)
        {
            new NotificationEvent(
                receiverClient,
                0,
                senderPid,
                (uint)NotificationEventType.Friends,
                1,
                senderPid,
                (uint)FriendsNotificationParam2.FriendshipRequested,
                0,
                senderName
                ).Send();
        }
    }
}
