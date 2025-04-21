using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using QuazalWV.Classes.Enums;

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
            FriendDBret query_result;
            switch (rmc.methodID)
            {
                case 4:
                    try
                    {

                        var reqAddFriendByName = (RMCPacketRequestFriendsService_AddFriendByNameWithDetails)rmc.request;
                        if (reqAddFriendByName.Invitee == reqAddFriendByName.Inviter)
                        {
                            Log.WriteLine(1, $"[ADD Friend Attempt]:  failed sending status 0", Color.Red);
                            reply = new RMCPacketResponseFriendsService_boolean(true);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, 0x80030070);
                            break;
                        }
                        Log.WriteLine(1, $"[Add friend Attempt user names]: {reqAddFriendByName.Invitee} {reqAddFriendByName.Inviter}");
                        invitee = DBHelper.GetUserByName(reqAddFriendByName.Invitee);
                        inviter = DBHelper.GetUserByName(reqAddFriendByName.Inviter);
                        Log.WriteLine(1, reqAddFriendByName.PayloadToString(), Color.Red);
                        query_result = FriendDBret.Default;
                        if (client.User.Name != inviter.Name)
                        {
                            Log.WriteLine(1, $"[HACK Attempt]: the invitee user name is not the same as invited user name real {client.User.Name} fake {inviter.Name}", Color.Red);
                            reply = new RMCPacketResponseFriendsService_boolean(true);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, 0x80030064);
                            break;
                        }
                        else if (invitee != null)
                        {
                            Log.WriteLine(1, $"[ADD Friend Attempt]:  {invitee.Pid} fake {inviter.Pid}", Color.Red);
                            query_result = DBHelper.AddFriendRequest(inviter.Pid, invitee.Pid, reqAddFriendByName.Details);
                        }
                        if (invitee == null)
                        {
                            Log.WriteLine(1, $"[ADD Friend Attempt]:  failed sending status 0", Color.Red);
                            reply = new RMCPacketResponseFriendsService_boolean(true);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, 0x80030064);
                            break;
                        }
                        Log.WriteLine(1, $"[ADD Friend Attempt]: add friend result is {query_result}", Color.Red);
                        reply = new RMCPacketResponseFriendsService_AddFriendByNameWithDetails(invitee.Pid, reqAddFriendByName.Invitee, query_result == FriendDBret.Succeeded);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, "An error occurred: " + ex.Message);
                        Log.WriteLine(1, "Stack Trace: " + ex.StackTrace);
                    }
                    break;
                case 5:
                    //TODO verify if the other player actually sent the invite or hacking attempt
                    var reqAcceptFriendship = (RMCPacketRequestFriendsService_AcceptFriendship)rmc.request;

                    DBHelper.RemoveRelationshipBoth(client.User.Pid, reqAcceptFriendship.Pid, (byte)PlayerRelationship.pending);

                    invitee = DBHelper.GetUserByID(reqAcceptFriendship.Pid);
                    if (invitee == null)
                    {
                        reply = new RMCPacketResponseFriendsService_boolean(true);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, 0x80030070);
                        Log.WriteLine(1, $"[ADD Friend Attempt]:  invitee is null so the name is not good", Color.Red);
                        break;

                    }
                    query_result = DBHelper.AddFriend(client.User.Pid, invitee.Pid, reqAcceptFriendship.Details);
                    if (query_result == FriendDBret.Succeeded)
                    {
                        reply = new RMCPacketResponseFriendsService_boolean(false);

                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);

                    }
                    else
                    {
                        reply = new RMCPacketResponseFriendsService_boolean(true);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, 0x80650014);
                    }
                    Log.WriteLine(1, $"[ADD Friend Attempt]: accept friend status  {query_result} details {reqAcceptFriendship.Details}", Color.Red);
                    break;
                case 6:
                    try
                    {
                        var reqDeclineFriendship = (RMCPacketRequestFriendsService_DeclineFriendship)rmc.request;


                        invitee = DBHelper.GetUserByID(reqDeclineFriendship.Pid);
                        if (invitee == null)
                        {
                            reply = new RMCPacketResponseFriendsService_boolean(true);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, 0x80030070);
                            break;

                        }
                        result = DBHelper.RemoveRelationshipBoth(client.User.Pid, reqDeclineFriendship.Pid, (byte)PlayerRelationship.pending);
                        if (result == true)
                        {
                            reply = new RMCPacketResponseFriendsService_boolean(true);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                        }
                        else
                        {
                            reply = new RMCPacketResponseFriendsService_boolean(true);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, 0x80030067);
                        }
                        Log.WriteLine(1, $"[Remove Friend Invite]: decline friend status  {result} ", Color.Red);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, "An error occurred: " + ex.Message);
                        Log.WriteLine(1, "Stack Trace: " + ex.StackTrace);
                    }
                    break;
                case 7:
                    try
                    {
                        var reqBlacklist = (RMCPacketRequestFriendsService_BlackList)rmc.request;
                        User blacklistee = DBHelper.GetUserByID(reqBlacklist.Pid);
                        if (blacklistee == null)
                        {
                            Log.WriteLine(1, $"[ADD Blacklist Attempt]: blacklisted person not existant", Color.Red);
                            reply = new RMCPacketResponseFriendsService_boolean(false);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, 0x80650019);
                            break;
                        }
                        query_result = DBHelper.AddBlacklistRequest(client.User.Pid, blacklistee.Pid, reqBlacklist.Details);
                        DBHelper.RemoveRelationshipBoth(client.User.Pid, blacklistee.Pid, (byte)PlayerRelationship.pending);
                        DBHelper.RemoveRelationshipBoth(client.User.Pid, blacklistee.Pid, (byte)PlayerRelationship.friend);
                        if (query_result != FriendDBret.Succeeded)
                        {
                            Log.WriteLine(1, $"[ADD Blacklist Attempt]: blacklisted person could not be done", Color.Red);
                            reply = new RMCPacketResponseFriendsService_BlackList(false);
                            RMC.SendResponseWithACK(client.udp, p, rmc, client, reply, true, 0x80650019);
                            break;
                        }
                        reply = new RMCPacketResponseFriendsService_BlackList(query_result == FriendDBret.Succeeded);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                        Log.WriteLine(1, $"[ADD Friend Attempt]: blacklist friend status  {query_result} ", Color.Red);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, "An error occurred: " + ex.Message);
                        Log.WriteLine(1, "Stack Trace: " + ex.StackTrace);
                    }
                    break;
                case 9:
                    try
                    {
                        var reqDeleteRel = (RMCPacketRequestFriendsService_ClearRelationship)rmc.request;
                        Log.WriteLine(1, $"pid is {reqDeleteRel.Pid}", Color.Red);
                        uint requestee_user_pid = reqDeleteRel.Pid;
                        uint current_pid = client.User.Pid;
                        DBHelper.RemoveRelationshipBoth(current_pid, requestee_user_pid, (byte)PlayerRelationship.pending);
                        Log.WriteLine(1, $"[ADD Friend Attempt]: cleared relationships blocked is only cleared one way {requestee_user_pid} {current_pid} ", Color.Red);
                        DBHelper.RemoveRelationshipOneSided(current_pid, requestee_user_pid, (byte)PlayerRelationship.blocked);
                        DBHelper.RemoveRelationshipBoth(current_pid, requestee_user_pid, (byte)PlayerRelationship.friend);
                        reply = new RMCPacketResponseFriendsService_ClearRelationship();
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                        Log.WriteLine(1, $"[ADD Friend Attempt]: cleared relationships blocked is only cleared one way ", Color.Red);

                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, "An error occurred: " + ex.Message);
                        Log.WriteLine(1, "Stack Trace: " + ex.StackTrace);
                    }
                    break;
                case 12:

                    try
                    {
                        var reqGetDetailedList = (RMCPacketRequestFriendsService_GetDetailedList)rmc.request;
                        Log.WriteLine(1, $"[RMC RelationshipList] client {client.User.Pid} requested  {reqGetDetailedList.ToString()}", Color.Red);
                        List<RelationshipDBData> rels = DBHelper.ReturnRelationships(client.User.Pid, reqGetDetailedList.Relationship, reqGetDetailedList.Reversed);
                        Log.WriteLine(1, $"[RMC RelationshipList] returned rels{rels.Count} requested ");
                        List<FriendData> friends = FriendsFromRelationships(rels, client, reqGetDetailedList.Relationship);
                        if (reqGetDetailedList.Relationship==1)
                        {
                            friends.Add(new FriendData
                            {
                                Pid = 4661,
                                Name = "Fwiend",
                                Relationship = 1,
                                Details = 0,
                                Status = "Online"
                            });
                        }
                        Log.WriteLine(1, $"[RMC RelationshipList] returned friens{friends.Count} requested ");
                        reply = new RMCPacketResponseFriendsService_GetDetailedList(friends);
                        RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(1, "An error occurred: " + ex.Message);
                        Log.WriteLine(1, "Stack Trace: " + ex.StackTrace);
                    }
                    break;

                case 13:
                    reply = new RMCPacketResponseFriendsService_GetRelationships();
                    RMC.SendResponseWithACK(client.udp, p, rmc, client, reply);
                    break;
                default:
                    Log.WriteLine(1, $"[RMC Friends] Error: Unknown Method {rmc.methodID}", Color.Red, client);
                    break;
            }

        }
        public static List<FriendData> FriendsFromRelationships(List<RelationshipDBData> rels, ClientInfo client, byte relationshipsearched)
        {
            FriendData tempfriend;
            User reluser;
            List<FriendData> friends = new List<FriendData>();
            //friends.Add(new FriendData
            //{
            //    Pid = 5,
            //    Name = "h",
            //    Relationship = relationshipsearched,
            //    Details = 0,
            //    Status = "Online"
            //});
            foreach (var relationship in rels)
            {

                if (client.User.Pid == relationship.Pidrequestor)
                {
                    tempfriend = new FriendData();
                    tempfriend.Status = "Offline";
                    tempfriend.Pid = relationship.Pidrequestee;
                    tempfriend.Relationship = relationship.Status;
                    tempfriend.Details = relationship.Details;
                    reluser = DBHelper.GetUserByID(relationship.Pidrequestee);
                    tempfriend.Name = reluser.Name;
                    foreach (ClientInfo clientiter in Global.Clients)
                    {
                        if (clientiter.User.Pid == relationship.Pidrequestee)
                            tempfriend.Status = "Online";
                    }
                    Log.WriteLine(1, $"[RMC Friends] Friend added  {tempfriend.ToString()}", Color.Red);
                    friends.Add(tempfriend);
                }
                else
                {
                    reluser = DBHelper.GetUserByID(relationship.Pidrequestor);
                    tempfriend = new FriendData();
                    tempfriend.Status = "Offline";
                    tempfriend.Pid = reluser.Pid;
                    tempfriend.Relationship = relationship.Status;
                    tempfriend.Details = relationship.Details;
                    tempfriend.Name = reluser.Name;
                    foreach (ClientInfo clientiter in Global.Clients)
                    {
                        if (clientiter.User.Pid == reluser.Pid)
                            tempfriend.Status = "Online";
                    }
                    Log.WriteLine(1, $"[RMC Friends] Friend added  {tempfriend.ToString()}", Color.Red);
                    friends.Add(tempfriend);
                }
            }
            return friends;
        }
    }
}
