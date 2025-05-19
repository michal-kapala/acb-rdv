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

        public static void FriendStatusChanged(ClientInfo receiverClient, uint senderPid, string senderName, bool online)
        {
            new NotificationEvent(
                receiverClient,
                0,
                senderPid,
                (uint)NotificationEventType.FriendStatusChange,
                1,
                senderPid,
                online ? 1u : 0u,
                0,
                senderName
                ).Send();
        }

        public static void GameInviteSent(ClientInfo receiverClient, uint senderPid, GameSessionInvitation invite)
        {
            new NotificationEvent(
                receiverClient,
                0,
                senderPid,
                (uint)NotificationEventType.GameSession,
                (uint)GameSessionNotificationSubtype.GameSessionNotif5,
                0,
                invite.Key.SessionId,
                invite.Key.TypeId,
                invite.Message
                ).Send();
        }
    }
}
