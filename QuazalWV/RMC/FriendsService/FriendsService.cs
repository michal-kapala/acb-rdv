using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

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

        public static void HandleRequest(QPacket p, RMCP rmc, ClientInfo client)
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
                        invitee = DBHelper.GetUserByName(reqAddFriendByName.Invitee);
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
                            dbResult = DBHelper.AddFriendRequest(client.User.Pid, invitee.Pid, reqAddFriendByName.Details);
                            switch (dbResult)
                            {
                                case DbRelationshipResult.UserBlocked:
                                    reply = new RMCPResponseEmpty();
                                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.Friends_CannotAddBlacklistedPlayer);
                                    break;
                                default:
                                    reply = new RMCPacketResponseFriendsService_AddFriendByNameWithDetails(invitee.Pid, reqAddFriendByName.Invitee);
                                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
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
                        inviter = DBHelper.GetUserByID(reqAcceptFriendship.Pid);
                        if (inviter == null)
                        {
                            Log.WriteLine(1, $"[RMC Friends] Inviter {reqAcceptFriendship.Pid} not found", Color.Red, client);
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.Friends_UserNotFound);
                        }
                        else
                        {
                            dbResult = DBHelper.AddFriend(inviter.Pid, client.User.Pid, reqAcceptFriendship.Details);
                            reply = new RMCPacketResponseFriendsService_AcceptFriendship(dbResult == DbRelationshipResult.Succeeded);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
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
                        inviter = DBHelper.GetUserByID(reqDeclineFriendship.Pid);
                        if (inviter == null)
                        {
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.Friends_UserNotFound);
                        }
                        else
                        {
                            result = DBHelper.RemoveRelationship(reqDeclineFriendship.Pid, client.User.Pid);
                            reply = new RMCPacketResponseFriendsService_DeclineFriendship(result);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
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
                        User blacklisted = DBHelper.GetUserByID(reqBlacklist.Pid);
                        if (blacklisted == null)
                        {
                            reply = new RMCPResponseEmpty();
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, (uint)QError.Friends_UserNotFound);
                        }
                        else
                        {
                            dbResult = DBHelper.AddBlacklistRequest(client.User.Pid, blacklisted.Pid, reqBlacklist.Details);
                            reply = new RMCPacketResponseFriendsService_BlackList(dbResult == DbRelationshipResult.Succeeded);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
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
                        result = DBHelper.RemoveRelationship(client.PID, reqClearRel.Pid);
                        reply = new RMCPacketResponseFriendsService_ClearRelationship(result);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
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
                        List<FriendData> friends = DBHelper.GetFriends(client.User.Pid, reqGetDetailedList.Relationship);
                        Log.WriteLine(1, $"[RMC Friends] GetDetailedList returned {friends.Count} friends for relationship {(PlayerRelationship)reqGetDetailedList.Relationship}", Color.Blue, client);
                        reply = new RMCPacketResponseFriendsService_GetDetailedList(friends);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
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
                        List<RelationshipData> rels = DBHelper.GetRelationshipData(client.User.Pid, reqGetRels.ResultRange.Size);
                        Log.WriteLine(1, $"[RMC Friends] GetRelationships returned {rels.Count} friend relationships", Color.Blue, client);
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
    }
}
