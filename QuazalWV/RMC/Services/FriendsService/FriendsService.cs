using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuazalWV
{
    public static class FriendsService
    {
        public static void ProcessRequest(Stream s, RMCP rmc)
        {
            switch (rmc.methodID)
            {
                case 4:
                    rmc.request = new RMCPacketRequestFriendsService_AddFriendByNameWithDetails(s);
                    break;
                case 5:
                    rmc.request = new RMCPacketRequestFriendsService_AcceptFriendship(s);
                    break;
                case 6:
                    rmc.request = new RMCPacketRequestFriendsService_DeclineFriendship(s);
                    break;
                case 7:
                    rmc.request = new RMCPacketRequestFriendsService_BlackList(s);
                    break;
                case 9:
                    rmc.request = new RMCPacketRequestFriendsService_ClearRelationship(s);
                    break;
                case 12:
                    rmc.request = new RMCPacketRequestFriendsService_GetDetailedList(s);
                    break;
                case 13:
                    rmc.request = new RMCPacketRequestFriendsService_GetRelationships(s);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Friends] Error: Unknown Method {rmc.methodID}", Color.Red);
                    break;
            }
        }

        public static void HandleRequest(PrudpPacket p, RMCP rmc, ClientInfo client)
        {
            RMCPResponse reply;
            User invitee, inviter;
            bool result;
            var dbResult = DbRelationshipResult.Default;
            switch (rmc.methodID)
            {
                case 4:
                    var reqAddFriendByName = (RMCPacketRequestFriendsService_AddFriendByNameWithDetails)rmc.request;
                    try
                    {
                        invitee = DbHelper.GetUserByName(reqAddFriendByName.Invitee);
                        if (reqAddFriendByName.Invitee == reqAddFriendByName.Inviter)
                        {
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.Friends_CannotAddYourselfAsFriend);
                        }
                        else if (invitee == null)
                        {
                            Log.WriteLine(1, $"[RMC Friends] Invitee {reqAddFriendByName.Invitee} not found", Color.Red, client);
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.Friends_UserNotFound);
                        }
                        else
                        {
                            dbResult = DbHelper.AddFriendRequest(client.User.Pid, invitee.Pid, reqAddFriendByName.Details);
                            switch (dbResult)
                            {
                                case DbRelationshipResult.UserBlocked:
                                    reply = new RMCPResponseEmpty();
                                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.Friends_CannotAddBlacklistedPlayer);
                                    break;
                                default:
                                    reply = new RMCPacketResponseFriendsService_AddFriendByNameWithDetails(invitee.Pid, reqAddFriendByName.Invitee);
                                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                                    // send instant invite notif
                                    var inviteeClient = Global.Clients.Find(c => c.User.Pid == invitee.Pid);
                                    if (inviteeClient != null)
                                        NotificationManager.FriendInviteReceived(inviteeClient, client.User.Pid, client.User.Name);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, $"[RMC Friends] AddFriendByNameWithDetails: {ex.Message}", Color.Red, client);
                    }
                    break;
                case 5:
                    // TODO: validate the invitation
                    var reqAcceptFriendship = (RMCPacketRequestFriendsService_AcceptFriendship)rmc.request;
                    try
                    {
                        inviter = DbHelper.GetUserByID(reqAcceptFriendship.Pid);
                        if (inviter == null)
                        {
                            Log.WriteLine(1, $"[RMC Friends] Inviter {reqAcceptFriendship.Pid} not found", Color.Red, client);
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.Friends_UserNotFound);
                        }
                        else
                        {
                            dbResult = DbHelper.AddFriend(inviter.Pid, client.User.Pid, reqAcceptFriendship.Details);
                            reply = new RMCPacketResponseFriendsService_AcceptFriendship(dbResult == DbRelationshipResult.Succeeded);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                            // send friend list update notifs
                            var inviterClient = Global.Clients.Find(c => c.User.Pid == inviter.Pid);
                            if (inviterClient != null)
                            {
                                // instant invite update notif
                                NotificationManager.FriendInviteAccepted(inviterClient, client.User.Pid, client.User.Name);
                                // mark inviter as online
                                NotificationManager.FriendStatusChanged(client, inviterClient.User.Pid, inviterClient.User.Name, true);
                            }
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, $"[RMC Friends] AcceptFriendship: {ex.Message}", Color.Red, client);
                    }
                    break;
                case 6:
                    var reqDeclineFriendship = (RMCPacketRequestFriendsService_DeclineFriendship)rmc.request;
                    try
                    {
                        inviter = DbHelper.GetUserByID(reqDeclineFriendship.Pid);
                        if (inviter == null)
                        {
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.Friends_UserNotFound);
                        }
                        else
                        {
                            result = DbHelper.RemoveRelationship(reqDeclineFriendship.Pid, client.User.Pid);
                            reply = new RMCPacketResponseFriendsService_DeclineFriendship(result);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                            // send invite declined notif
                            ClientInfo inviterClient = Global.Clients.Find(c => c.User.Pid == reqDeclineFriendship.Pid);
                            if (inviterClient != null)
                                NotificationManager.FriendRemoved(inviterClient, client.User.Pid, client.User.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, $"[RMC Friends] DeclineFriendship: {ex.Message}", Color.Red, client);
                    }
                    break;
                case 7:
                    var reqBlacklist = (RMCPacketRequestFriendsService_BlackList)rmc.request;
                    try
                    {
                        User blacklisted = DbHelper.GetUserByID(reqBlacklist.Pid);
                        if (blacklisted == null)
                        {
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.Friends_UserNotFound);
                        }
                        else
                        {
                            dbResult = DbHelper.AddBlacklistRequest(client.User.Pid, blacklisted.Pid, reqBlacklist.Details);
                            reply = new RMCPacketResponseFriendsService_BlackList(dbResult == DbRelationshipResult.Succeeded);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                            // remove from friend list when blocked
                            ClientInfo inviterClient = Global.Clients.Find(c => c.User.Pid == reqBlacklist.Pid);
                            if (inviterClient != null && dbResult == DbRelationshipResult.Succeeded)
                                NotificationManager.FriendRemoved(inviterClient, client.User.Pid, client.User.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, $"[RMC Friends] BlackList: {ex.Message}", Color.Red, client);
                    }
                    break;
                case 9:
                    var reqClearRel = (RMCPacketRequestFriendsService_ClearRelationship)rmc.request;
                    try
                    {
                        result = DbHelper.RemoveRelationship(client.User.Pid, reqClearRel.Pid);
                        reply = new RMCPacketResponseFriendsService_ClearRelationship(result);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                        // send removed from friends notif
                        ClientInfo inviterClient = Global.Clients.Find(c => c.User.Pid == reqClearRel.Pid);
                        if (inviterClient != null && result)
                            NotificationManager.FriendRemoved(inviterClient, client.User.Pid, client.User.Name);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, $"[RMC Friends] ClearRelationship: {ex.Message}", Color.Red, client);
                    }
                    break;
                case 12:
                    var reqGetDetailedList = (RMCPacketRequestFriendsService_GetDetailedList)rmc.request;
                    try
                    {
                        var relationships = DbHelper.GetRelationships(client.User.Pid, reqGetDetailedList.Relationship);
                        var friends = new List<FriendData>();
                        uint otherPid;
                        User otherUser;
                        ClientInfo friendClient;
                        bool online, inviteNotif;
                        foreach (var rel in relationships)
                        {
                            otherPid = rel.RequesterPid == client.User.Pid ? rel.RequesteePid : rel.RequesterPid;
                            otherUser = DbHelper.GetUserByID(otherPid);
                            friendClient = Global.Clients.Find(c => c.User.Pid == otherPid);
                            online = friendClient != null;
                            inviteNotif = rel.Type == PlayerRelationship.Pending && otherPid == rel.RequesterPid;
                            friends.Add(new FriendData(rel, otherUser, online, inviteNotif));
                            // send 'is now online' notifs to friends
                            if (online && rel.Type == PlayerRelationship.Friend)
                                NotificationManager.FriendStatusChanged(friendClient, client.User.Pid, client.User.Name, true);
                        }
                        reply = new RMCPacketResponseFriendsService_GetDetailedList(friends);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                        // send friend invite notifs on logon (3rd request)
                        if ((PlayerRelationship)reqGetDetailedList.Relationship == PlayerRelationship.Pending && reqGetDetailedList.Reversed)
                            Task.Run(async () => await SendLogonFriendInvites(client, friends));

                        // send game invite notifs on logon (4th request)
                        if ((PlayerRelationship)reqGetDetailedList.Relationship == PlayerRelationship.Blocked)
                            Task.Run(async () => await SendLogonGameInvites(client));
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, $"[RMC Friends] GetDetailedList: {ex.Message}", Color.Red, client);
                    }
                    break;
                case 13:
                    var reqGetRels = (RMCPacketRequestFriendsService_GetRelationships)rmc.request;
                    try
                    {
                        List<RelationshipData> rels = DbHelper.GetRelationshipData(client.User.Pid, reqGetRels.ResultRange.Size);
                        reply = new RMCPacketResponseFriendsService_GetRelationships(rels);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, $"[RMC Friends] GetRelationships: {ex.Message}", Color.Red, client);
                    }
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Friends] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }
        }

        private async static Task SendLogonFriendInvites(ClientInfo client, List<FriendData> friends)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            foreach (FriendData friend in friends)
            {
                if (friend != null && friend.InviteNotif)
                    NotificationManager.FriendInviteReceived(client, friend.Pid, friend.Name);
            }
        }

        private async static Task SendLogonGameInvites(ClientInfo client)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            var gameInvites = DbHelper.GetGameInvites(client.User.Pid);
            foreach (var invite in gameInvites)
            {
                var host = Global.Clients.Find(c => c.User.Pid == invite.Inviter);
                var ses = Global.Sessions.Find(s => s.Key.SessionId == invite.Invitation.Key.SessionId);
                if (host != null && ses != null)
                {
                    if (host.User != null)
                    {
                        uint pid = ses.PublicPids.Find(id => id == host.User.Pid);
                        if (pid == 0)
                            pid = ses.PrivatePids.Find(id => id == host.User.Pid);
                        if (pid == host.User.Pid)
                            NotificationManager.GameInviteSent(client, invite.Inviter, invite.Invitation);
                    }
                }
            }
            if (gameInvites.Count > 0)
                DbHelper.DeleteGameInvites(client.User.Pid);
        }
    }
}
