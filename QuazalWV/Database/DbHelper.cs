using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;

namespace QuazalWV
{
    public static class DbHelper
    {
        public static SQLiteConnection connection;

        public static void Init()
        {
            connection = new SQLiteConnection
            {
                ConnectionString = "Data Source=database.sqlite"
            };
            connection.Open();
            Log.WriteLine(1, "DB loaded...", LogSource.DB);
        }

        public static List<List<string>> GetQueryResults(string query)
        {
            List<List<string>> result = new List<List<string>>();
            SQLiteCommand command = new SQLiteCommand(query, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                List<string> entry = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                    entry.Add(reader[i].ToString());
                result.Add(entry);
            }
            reader.Close();
            reader.Dispose();
            command.Dispose();
            return result;
        }

        public static DbRelationshipResult AddFriendRequest(uint requester, uint requestee, uint details)
        {
            // verify users exist beforehand
            string query = "SELECT * FROM relationships WHERE (requester = @requester AND requestee = @requestee) OR (requester = @requestee AND requestee = @requester)";
            List<PlayerRelationship> forbidden = new List<PlayerRelationship> { PlayerRelationship.Pending, PlayerRelationship.Blocked };
            List<Relationship> relations = new List<Relationship>();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@requester", requester);
                command.Parameters.AddWithValue("@requestee", requestee);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var relationship = new Relationship
                        {
                            RequesterPid = Convert.ToUInt32(reader.GetInt32(0)),
                            RequesteePid = Convert.ToUInt32(reader.GetInt32(1)),
                            Type = (PlayerRelationship)reader.GetByte(2),
                            Details = Convert.ToUInt32(reader.GetInt32(3))
                        };
                        relations.Add(relationship);
                        if (forbidden.Contains(relationship.Type))
                        {
                            if (relationship.Type == PlayerRelationship.Blocked)
                                return DbRelationshipResult.UserBlocked;
                            return DbRelationshipResult.Failed;
                        }
                    }
                }

                if (relations.Count == 0)
                {
                    string insertQuery = "INSERT INTO relationships (requester, requestee, type, details) VALUES (@requester, @requestee, @type, @details)";
                    using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@requester", requester);
                        insertCommand.Parameters.AddWithValue("@requestee", requestee);
                        insertCommand.Parameters.AddWithValue("@type", PlayerRelationship.Pending);
                        insertCommand.Parameters.AddWithValue("@details", details);
                        int rowsAffected = insertCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                            return DbRelationshipResult.Succeeded;
                    }
                    return DbRelationshipResult.Failed;
                }
                else
                {
                    Log.WriteLine(1, $"The players {requester} and {requestee} are already friends", LogSource.DB, Color.Orange);
                    return DbRelationshipResult.AlreadyPresent;
                }
            }
        }

        public static DbRelationshipResult AddBlacklistRequest(uint requester, uint requestee, uint details)
        {
            // verify users exist beforehand
            string query = "SELECT * FROM relationships WHERE (requester = @requester AND requestee = @requestee) OR (requester = @requestee AND requestee = @requester)";
            List<PlayerRelationship> forbidden = new List<PlayerRelationship> { PlayerRelationship.Blocked };
            List<Relationship> relations = new List<Relationship>();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@requester", requester);
                command.Parameters.AddWithValue("@requestee", requestee);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var relationship = new Relationship
                        {
                            RequesterPid = Convert.ToUInt32(reader.GetInt32(0)),
                            RequesteePid = Convert.ToUInt32(reader.GetInt32(1)),
                            Type = (PlayerRelationship)Convert.ToByte(reader.GetInt32(2)),
                            Details = Convert.ToUInt32(reader.GetInt32(3))
                        };
                        relations.Add(relationship);
                        if (forbidden.Contains(relationship.Type))
                        {
                            Log.WriteLine(1, "Blacklisting failed, an entry already exists", LogSource.DB, Color.Orange);
                            return DbRelationshipResult.UserBlocked;
                        }
                    }
                }

                if (relations.Count == 1 && relations.First().Type != PlayerRelationship.Blocked)
                {
                    string sql = "UPDATE relationships SET requester = @requester, requestee = @requestee, type = @type, details = @details WHERE (requester = @requester AND requestee = @requestee) OR (requester = @requestee AND requestee = @requester)";
                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@type", PlayerRelationship.Blocked);
                        cmd.Parameters.AddWithValue("@details", details);
                        cmd.Parameters.AddWithValue("@requester", requester);
                        cmd.Parameters.AddWithValue("@requestee", requestee);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Log.WriteLine(1, $"Player {requester} blacklisted {requestee}", LogSource.DB, Color.Orange);
                            return DbRelationshipResult.Succeeded;
                        }
                        return DbRelationshipResult.Failed;
                    }
                }
                else
                {
                    Log.WriteLine(1, "One player already blacklisted the other or they dont have any relationship", LogSource.DB, Color.Orange);
                    return DbRelationshipResult.UserBlocked;
                }
            }
        }

        public static DbRelationshipResult AddFriend(uint requester, uint requestee, uint details)
        {
            string querySql = "SELECT * FROM relationships WHERE (requester = @requester AND requestee = @requestee) OR (requester = @requestee AND requestee = @requester)";
            List<PlayerRelationship> forbidden = new List<PlayerRelationship> { PlayerRelationship.Friend };
            List<Relationship> relations = new List<Relationship>();
            using (var query = new SQLiteCommand(querySql, connection))
            {
                query.Parameters.AddWithValue("@requester", requester);
                query.Parameters.AddWithValue("@requestee", requestee);
                using (var reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var relationship = new Relationship
                        {
                            RequesterPid = Convert.ToUInt32(reader.GetInt32(0)),
                            RequesteePid = Convert.ToUInt32(reader.GetInt32(1)),
                            Type = (PlayerRelationship)reader.GetByte(2),
                            Details = Convert.ToUInt32(reader.GetInt32(3))
                        };
                        relations.Add(relationship);
                        if (forbidden.Contains(relationship.Type))
                        {
                            Log.WriteLine(1, $"Players {requester} and {requestee} are already friends", LogSource.DB, Color.Orange);
                            return DbRelationshipResult.AlreadyPresent;
                        }
                    }
                }

                if (relations.Count == 1 && relations.First().Type == PlayerRelationship.Pending)
                {
                    string sql = "UPDATE relationships SET type = @type, details = @details WHERE (requester = @requester AND requestee = @requestee) OR (requester = @requestee AND requestee = @requester)";
                    using (var cmd = new SQLiteCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@type", PlayerRelationship.Friend);
                        cmd.Parameters.AddWithValue("@details", details);
                        cmd.Parameters.AddWithValue("@requester", requester);
                        cmd.Parameters.AddWithValue("@requestee", requestee);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0 ? DbRelationshipResult.Succeeded : DbRelationshipResult.Failed;
                    }
                }
                else
                {
                    Log.WriteLine(1, $"Players {requester} and {requestee} are already friends", LogSource.DB, Color.Orange);
                    return DbRelationshipResult.AlreadyPresent;
                }
            }
        }

        public static List<Relationship> GetRelationships(uint pid, byte type)
        {
            switch((PlayerRelationship)type)
            {
                case PlayerRelationship.Friend:
                    return GetSymmetric(pid, (byte)PlayerRelationship.Friend);
                case PlayerRelationship.Pending:
                    return GetInvites(pid);
                case PlayerRelationship.Blocked:
                    return GetSymmetric(pid, (byte)PlayerRelationship.Blocked);
                default:
                    Log.WriteLine(1, $"Unknown relationship type {type}", LogSource.DB, Color.Red);
                    return new List<Relationship>();
            }
        }

        public static List<RelationshipData> GetRelationshipData(uint pid, uint maxSize)
        {
            // TODO: check type
            var relationships = GetRelationships(pid, (byte)PlayerRelationship.Friend);
            var rdata = new List<RelationshipData>();
            uint otherPid;
            User otherUser;
            bool online;
            foreach (var rel in relationships)
            {
                otherPid = rel.RequesterPid == pid ? rel.RequesteePid : rel.RequesterPid;
                otherUser = GetUserByID(otherPid);
                online = Global.Clients.Find(c => c.User.Pid == otherPid) != null;
                rdata.Add(new RelationshipData(rel, otherUser, online));
                if (rdata.Count == maxSize)
                    break;
            }
            return rdata;
        }

        public static List<Relationship> GetInvites(uint pid)
        {
            string query = $"SELECT * FROM relationships WHERE (requester = @pid OR requestee = @pid) AND type = {(uint)PlayerRelationship.Pending}";
            List<Relationship> relations = new List<Relationship>();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@pid", pid);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var relationship = new Relationship
                        {
                            RequesterPid = Convert.ToUInt32(reader.GetInt32(0)),
                            RequesteePid = Convert.ToUInt32(reader.GetInt32(1)),
                            Type = (PlayerRelationship)reader.GetByte(2),
                            Details = Convert.ToUInt32(reader.GetInt32(3))
                        };
                        relations.Add(relationship);
                    }
                }
            }
            return relations;
        }

        public static List<Relationship> GetSymmetric(uint pid, byte type)
        {
            string query;
            if ((PlayerRelationship)type == PlayerRelationship.Friend)
                query = $"SELECT * FROM relationships WHERE (requester = @pid OR requestee = @pid) AND type = @type";
            else
                query = $"SELECT * FROM relationships WHERE requester = @pid AND type = @type";
            List<Relationship> relations = new List<Relationship>();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@pid", pid);
                command.Parameters.AddWithValue("@type", type);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var relationship = new Relationship
                        {
                            RequesterPid = Convert.ToUInt32(reader.GetInt32(0)),
                            RequesteePid = Convert.ToUInt32(reader.GetInt32(1)),
                            Type = (PlayerRelationship)reader.GetByte(2),
                            Details = Convert.ToUInt32(reader.GetInt32(3))
                        };
                        relations.Add(relationship);
                    }
                }
            }
            return relations;
        }

        public static bool RemoveRelationship(uint requester, uint requestee)
        {
            string query = "DELETE FROM relationships WHERE (requester = @requester AND requestee = @requestee) OR (requester = @requestee AND requestee = @requester)";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@requester", requester);
                command.Parameters.AddWithValue("@requestee", requestee);
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public static User GetUserByName(string name)
        {
            User result = null;
            string query = @"SELECT * FROM users WHERE name = @name";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@name", name);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = new User
                        {
                            Pid = Convert.ToUInt32(reader.GetInt32(reader.GetOrdinal("pid"))),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Password = reader.GetString(reader.GetOrdinal("password")),
                            UbiId = reader.GetString(reader.GetOrdinal("password")),
                            Email = reader.GetString(reader.GetOrdinal("email")),
                            CountryCode = reader.GetString(reader.GetOrdinal("country_code")),
                            PrefLang = reader.GetString(reader.GetOrdinal("pref_lang"))
                        };
                        return result;
                    }
                }
            }
            return result;
        }

        public static User GetUserByID(uint pid)
        {
            User result = null;
            string query = @"SELECT * FROM users WHERE pid=@pid";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@pid", pid);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = new User
                        {
                            Pid = Convert.ToUInt32(reader.GetInt32(reader.GetOrdinal("pid"))),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Password = reader.GetString(reader.GetOrdinal("password")),
                            UbiId = reader.GetString(reader.GetOrdinal("password")),
                            Email = reader.GetString(reader.GetOrdinal("email")),
                            CountryCode = reader.GetString(reader.GetOrdinal("country_code")),
                            PrefLang = reader.GetString(reader.GetOrdinal("pref_lang"))
                        };
                        return result;
                    }
                }
            }
            return result;
        }

        public static List<Privilege> GetPrivileges(string locale)
        {
            List<Privilege> privileges = new List<Privilege>();
            List<List<string>> results = GetQueryResults($"SELECT * FROM privileges WHERE locale='{locale}'");
            if (results.Count == 0)
                Log.WriteLine(1, $"Unknown locale: {locale}", LogSource.DB, Color.Red);

            foreach (List<string> entry in results)
            {
                privileges.Add(
                    new Privilege()
                    {
                        Id = Convert.ToUInt32(entry[0]),
                        Description = entry[1]
                    }
                );
            }
            return privileges;
        }

        public static List<UplayReward> GetRewards(string platform)
        {
            List<UplayReward> rewards = new List<UplayReward>();
            List<List<string>> results = GetQueryResults($"SELECT * FROM rewards");
            if (results.Count < 6)
                Log.WriteLine(1, $"Rewards missing", LogSource.DB, Color.Red);

            foreach (List<string> entry in results)
            {
                rewards.Add(
                    new UplayReward(platform)
                    {
                        Code = entry[0],
                        Name = entry[1],
                        Description = entry[2],
                        Value = Convert.ToUInt32(entry[3]),
                    }
                );
            }
            return rewards;
        }

        /// <summary>
        /// Returns all available telemetry event types (tags).
        /// </summary>
        /// <returns></returns>
        public static List<string> GetTags()
        {
            var tags = new List<string>();
            var results = GetQueryResults("SELECT * FROM tags");
            foreach (var entry in results)
                tags.Add(entry[0]);

            return tags;
        }

        public static bool SaveTag(TelemetryTag tag)
        {
            string sql = $"INSERT INTO telemetry_tags (tracking_id, tag, attr, dtime)" +
                         $"VALUES ({tag.TrackingId},'{tag.Tag}','{tag.Attributes}',{tag.DeltaTime})";

            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            try
            {
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception e)
            {
                Log.WriteLine(1, $"{e}", LogSource.DB, Color.Red);
                return false;
            }
        }

        public static bool AddMessage(TextMessage msg)
        {
            int rowsAffected;
            string sql = $"INSERT INTO messages (recipient_pid, recipient_type, parent_id, sender_pid, reception_time, lifetime, flags, subject, sender_name, body, delivered) VALUES (@recipient_pid, @recipient_type, @parent_id, @sender_pid, @reception_time, @lifetime, @flags, @subject, @sender_name, @body, @delivered)";
            using (var insertCommand = new SQLiteCommand(sql, connection))
            {
                insertCommand.Parameters.AddWithValue("@id", msg.Id);
                insertCommand.Parameters.AddWithValue("@recipient_pid", msg.RecipientId);
                insertCommand.Parameters.AddWithValue("@recipient_type", msg.RecipientType);
                insertCommand.Parameters.AddWithValue("@parent_id", msg.ParentId);
                insertCommand.Parameters.AddWithValue("@sender_pid", msg.SenderId);
                insertCommand.Parameters.AddWithValue("@reception_time", msg.ReceptionTime.RawTime);
                insertCommand.Parameters.AddWithValue("@lifetime", msg.Lifetime);
                insertCommand.Parameters.AddWithValue("@flags", msg.Flags);
                insertCommand.Parameters.AddWithValue("@subject", msg.Subject);
                insertCommand.Parameters.AddWithValue("@sender_name", msg.SenderName);
                insertCommand.Parameters.AddWithValue("@body", msg.Body);
                insertCommand.Parameters.AddWithValue("@delivered", false);
                try
                {
                    rowsAffected = insertCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.WriteLine(1, $"{ex}", LogSource.DB, Color.Red);
                    return false;
                }
            }
            return rowsAffected > 0;
        }

        public static List<TextMessage> GetMessagesByIds(List<uint> ids)
        {
            List<TextMessage> result = new List<TextMessage>();
            var placeholders = new List<string>();
            for (int i = 0; i < ids.Count; i++)
                placeholders.Add($"@id{i}");
            
            string sql = $"SELECT * FROM messages WHERE id IN ({string.Join(", ", placeholders)})";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                for (int i = 0; i < ids.Count; i++)
                    cmd.Parameters.AddWithValue($"@id{i}", ids[i]);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var msg = new TextMessage()
                        {
                            Id = Convert.ToUInt32(reader.GetInt32(reader.GetOrdinal("id"))),
                            RecipientId = (uint)(reader.GetInt32(reader.GetOrdinal("recipient_pid"))),
                            RecipientType = (uint)(reader.GetInt32(reader.GetOrdinal("recipient_type"))),
                            ParentId = (uint)(reader.GetInt32(reader.GetOrdinal("parent_id"))),
                            SenderId = (uint)(reader.GetInt32(reader.GetOrdinal("sender_pid"))),
                            ReceptionTime = new QDateTime(Convert.ToUInt64(reader.GetInt64(reader.GetOrdinal("reception_time")))),
                            Lifetime = (uint)(reader.GetInt32(reader.GetOrdinal("lifetime"))),
                            Flags = (uint)(reader.GetInt32(reader.GetOrdinal("flags"))),
                            Subject = reader.GetString(reader.GetOrdinal("subject")),
                            SenderName = reader.GetString(reader.GetOrdinal("sender_name")),
                            Body = reader.GetString(reader.GetOrdinal("body"))
                        };
                        msg.Header = new AnyDataHeader("TextMessage", msg.GetSize());
                        result.Add(msg);
                    }
                }
            }
            return result;
        }

        public static List<TextMessage> GetPendingMessages(MessageRecipient recipient)
        {
            List<TextMessage> result = new List<TextMessage>();
            string sql = "SELECT * FROM messages WHERE recipient_pid = @recipient_pid AND delivered = 0";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@recipient_pid", recipient.Pid);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var header = new TextMessage
                        {
                            Id = Convert.ToUInt32(reader.GetInt32(reader.GetOrdinal("id"))),
                            RecipientId = (uint)(reader.GetInt32(reader.GetOrdinal("recipient_pid"))),
                            RecipientType = (uint)(reader.GetInt32(reader.GetOrdinal("recipient_type"))),
                            ParentId = (uint)(reader.GetInt32(reader.GetOrdinal("parent_id"))),
                            SenderId = (uint)(reader.GetInt32(reader.GetOrdinal("sender_pid"))),
                            ReceptionTime = new QDateTime(Convert.ToUInt64(reader.GetInt64(reader.GetOrdinal("reception_time")))),
                            Lifetime = (uint)(reader.GetInt32(reader.GetOrdinal("lifetime"))),
                            Flags = (uint)(reader.GetInt32(reader.GetOrdinal("flags"))),
                            Subject = reader.GetString(reader.GetOrdinal("subject")),
                            SenderName = reader.GetString(reader.GetOrdinal("sender_name")),
                            Body = reader.GetString(reader.GetOrdinal("body")),
                        };
                        result.Add(header);
                    }
                }
            }
            return result;
        }

        public static bool UpdateDeliveredMessages(List<uint> ids)
        {
            int rowsAffected;
            var placeholders = new List<string>();
            for (int i = 0; i < ids.Count; i++)
                placeholders.Add($"@id{i}");

            string sql = $"UPDATE messages SET delivered = 1 WHERE id IN ({string.Join(", ", placeholders)})";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                for (int i = 0; i < ids.Count; i++)
                    cmd.Parameters.AddWithValue($"@id{i}", ids[i]);
                try
                {
                    rowsAffected = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.WriteLine(1, $"{ex}", LogSource.DB, Color.Red);
                    return false;
                }
            }
            return rowsAffected == ids.Count;
        }

        public static bool AddGameInvites(GameSessionKey sesKey, uint inviterPid, uint inviteePid, string message)
        {
            int rowsAffected;
            string sql = "INSERT INTO game_invites (inviter, invitee, session_type, session_id, message) VALUES (@inviter, @invitee, @session_type, @session_id, @message)";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@inviter", inviterPid);
                cmd.Parameters.AddWithValue("@invitee", inviteePid);
                cmd.Parameters.AddWithValue("@session_type", sesKey.TypeId);
                cmd.Parameters.AddWithValue("@session_id", sesKey.SessionId);
                cmd.Parameters.AddWithValue("@message", message);
                try
                {
                    rowsAffected = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.WriteLine(1, $"{ex}", LogSource.DB, Color.Red);
                    return false;
                }
            }
            return rowsAffected > 0;
        }

        public static List<GameInvite> GetGameInvites(uint inviteePid)
        {
            List<GameInvite> invites = new List<GameInvite>();
            string sql = "SELECT * FROM game_invites WHERE invitee = @invitee";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@invitee", inviteePid);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var key = new GameSessionKey
                        {
                            TypeId = (uint)(reader.GetInt32(reader.GetOrdinal("session_type"))),
                            SessionId = (uint)(reader.GetInt32(reader.GetOrdinal("session_id")))
                        };

                        var invitation = new GameSessionInvitation
                        {
                            Key = key,
                            Recipients = new List<uint> { inviteePid },
                            Message = reader.GetString(reader.GetOrdinal("message"))
                        };

                        var invite = new GameInvite
                        {
                            Id = (uint)(reader.GetInt32(reader.GetOrdinal("id"))),
                            Inviter = (uint)(reader.GetInt32(reader.GetOrdinal("inviter"))),
                            Invitation = invitation
                        };
                        invites.Add(invite);
                    }
                }
            }
            return invites;
        }

        public static bool DeleteGameInvites(uint inviteePid)
        {
            int rowsAffected;
            string sql = "DELETE FROM game_invites WHERE invitee = @invitee";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@invitee", inviteePid);
                try
                {
                    rowsAffected = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.WriteLine(1, $"{ex}", LogSource.DB, Color.Red);
                    return false;
                }
            }
            return rowsAffected > 0;
        }
    }
}
