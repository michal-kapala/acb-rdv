using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;

namespace QuazalWV
{
    public static class DBHelper
    {
        public static SQLiteConnection connection = new SQLiteConnection();

        public static void Init()
        {
            connection.ConnectionString = "Data Source=database.sqlite";
            connection.Open();
            Log.WriteLine(1, "DB loaded...");
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
                    Log.WriteLine(1, $"[DB] The players {requester} and {requestee} are already friends", Color.Orange);
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
                            Log.WriteLine(1, "[DB] Blacklisting failed, an entry already exists", Color.Orange);
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
                            Log.WriteLine(1, $"[DB] Player {requester} blacklisted {requestee}", Color.Orange);
                            return DbRelationshipResult.Succeeded;
                        }
                        return DbRelationshipResult.Failed;
                    }
                }
                else
                {
                    Log.WriteLine(1, "[DB] One player already blacklisted the other or they dont have any relationship", Color.Orange);
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
                            Log.WriteLine(1, $"[DB] Players {requester} and {requestee} are already friends", Color.Orange);
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
                    Log.WriteLine(1, $"[DB] Players {requester} and {requestee} are already friends", Color.Orange);
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
                    Log.WriteLine(1, $"[DB] Unknown relationship type {type}", Color.Red);
                    return new List<Relationship>();
            }
        }

        public static List<FriendData> GetFriends(uint pid, byte type)
        {
            var relationships = GetRelationships(pid, type);
            var fdata = new List<FriendData>();
            uint otherPid;
            User otherUser;
            bool online, inviteNotif;
            foreach (var rel in relationships)
            {
                otherPid = rel.RequesterPid == pid ? rel.RequesteePid : rel.RequesterPid;
                otherUser = GetUserByID(otherPid);
                online = Global.Clients.Find(c => c.User.Pid == otherPid) != null;
                inviteNotif = rel.Type == PlayerRelationship.Pending && otherPid == rel.RequesterPid;
                fdata.Add(new FriendData(rel, otherUser, online, inviteNotif));
            }
            return fdata;
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
                Log.WriteLine(1, $"[RMC Privileges] Unknown locale: {locale}", Color.Red);

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
                Log.WriteLine(1, $"[RMC UplayWin] Rewards missing", Color.Red);

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
                Log.WriteLine(1, $"[DB] {e}", Color.Red);
                return false;
            }
        }
    }
}
