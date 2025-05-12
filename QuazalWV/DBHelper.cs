using QuazalWV.Classes.Enums;
using System;
using System.Collections;
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

        public static FriendDBret AddFriendRequest(uint requestor, uint requestee, uint detail)
        {//verify users exist beforehand
            string query = "SELECT * FROM relationship WHERE (uidrequestor = @requestor AND uidrequestee = @requestee ) OR (uidrequestee = @requestee  AND uidrequestee = @requestor )";
            List<PlayerRelationship> ignorestuff = new List<PlayerRelationship> { PlayerRelationship.blocked, PlayerRelationship.pending };
            List<RelationshipDBData> relations = new List<RelationshipDBData>();
            bool ConditionsMet = true;
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@requestor", requestor);
                command.Parameters.AddWithValue("@requestee", requestee);

                using (var reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var relationship = new RelationshipDBData
                        {
                            Pidrequestor = Convert.ToUInt32(reader.GetInt32(0)),
                            Pidrequestee = Convert.ToUInt32(reader.GetInt32(1)),
                            Status = reader.GetByte(2),
                            Details = Convert.ToUInt32(reader.GetInt32(3))

                        };
                        relations.Add(relationship);
                        if (ignorestuff.Contains((PlayerRelationship)relationship.Status))
                        {
                            Log.WriteLine(1, "[RMC FriendInvite] Invite will not be sent some invites are still pending or it is already your friend or blocked", Color.Orange);
                            ConditionsMet = false;
                            return FriendDBret.Failed;
                        }
                    }
                }


                // If less than 2 IDs found, insert both into BackupTable
                if (ConditionsMet == true)
                {
                    string insertQuery = "INSERT INTO relationship (uidrequestor, uidrequestee , status, detail_value ) VALUES (@id1,@id2,0,@detail),(@id1,@id2,2,@detail)";
                    using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@id1", requestor);
                        insertCommand.Parameters.AddWithValue("@id2", requestee);
                        insertCommand.Parameters.AddWithValue("@detail", detail);
                        int rowsAffected = insertCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                            return FriendDBret.Succeeded;
                    }

                    Log.WriteLine(1, "[RMC FriendInvite] Sent The Invite:", Color.Orange);
                }
                else
                {
                    Log.WriteLine(1, "[RMC FriendInvite] Either You invited Him or he invited you before:", Color.Orange);
                    return FriendDBret.AlreadyPresent;

                }
            }
            return FriendDBret.Failed;
        }


        public static FriendDBret AddBlacklistRequest(uint requestor, uint requestee, uint detail)
        {//verify users exist beforehand
            string query = "SELECT * FROM relationship WHERE (uidrequestor = @requestor AND uidrequestee = @requestee )";
            List<PlayerRelationship> ignorestuff = new List<PlayerRelationship> { PlayerRelationship.blocked };
            List<RelationshipDBData> relations = new List<RelationshipDBData>();
            bool ConditionsMet = true;
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@requestor", requestor);
                command.Parameters.AddWithValue("@requestee", requestee);

                using (var reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var relationship = new RelationshipDBData
                        {
                            Pidrequestor = Convert.ToUInt32(reader.GetInt32(0)),
                            Pidrequestee = Convert.ToUInt32(reader.GetInt32(1)),
                            Status = reader.GetByte(2),
                            Details = Convert.ToUInt32(reader.GetInt32(3))

                        };
                        relations.Add(relationship);
                        if (ignorestuff.Contains((PlayerRelationship)relationship.Status))
                        {
                            Log.WriteLine(1, "[RMC FriendInvite] Blacklist will not be sent, it is already present", Color.Orange);
                            ConditionsMet = false;
                            return FriendDBret.UserBlocked;
                        }
                    }
                }


                // If less than 2 IDs found, insert both into BackupTable
                if (ConditionsMet == true)
                {
                    string insertQuery = "INSERT INTO relationship (uidrequestor, uidrequestee , status, detail_value ) VALUES (@id1,@id2,@status,@detail)";
                    using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@id1", requestor);
                        insertCommand.Parameters.AddWithValue("@id2", requestee);
                        insertCommand.Parameters.AddWithValue("@detail", detail);
                        insertCommand.Parameters.AddWithValue("@status", PlayerRelationship.blocked);

                        int rowsAffected = insertCommand.ExecuteNonQuery();
                        if (rowsAffected > 0)
                            return FriendDBret.Succeeded;
                    }

                    Log.WriteLine(1, "[RMC FriendInvite] Sent The Blacklist:", Color.Orange);
                }
                else
                {
                    Log.WriteLine(1, "[RMC FriendInvite] Either You Blacklisted him before or unk error", Color.Orange);
                    return FriendDBret.UserBlocked;

                }
            }
            return FriendDBret.Failed;
        }



        internal static FriendDBret AddFriend(uint requestor, uint requestee, uint detail)
        {
            string query = "SELECT * FROM relationship WHERE (uidrequestor = @requestor AND uidrequestee = @requestee )";
            List<PlayerRelationship> ignorestuff = new List<PlayerRelationship> { PlayerRelationship.friend };
            List<RelationshipDBData> relations = new List<RelationshipDBData>();
            bool ConditionsMet = true;
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@requestor", requestor);
                command.Parameters.AddWithValue("@requestee", requestee);

                using (var reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var relationship = new RelationshipDBData
                        {
                            Pidrequestor = Convert.ToUInt32(reader.GetInt32(0)),
                            Pidrequestee = Convert.ToUInt32(reader.GetInt32(1)),
                            Status = reader.GetByte(2),
                            Details = Convert.ToUInt32(reader.GetInt32(3))

                        };
                        relations.Add(relationship);
                        if (ignorestuff.Contains((PlayerRelationship)relationship.Status))
                        {
                            Log.WriteLine(1, "[RMC FriendInvite] Already your friend", Color.Orange);
                            ConditionsMet = false;
                            return FriendDBret.AlreadyPresent;
                        }
                    }
                }


                // If less than 2 IDs found, insert both into BackupTable
                if (ConditionsMet == true)
                {
                    string insertQuery = "INSERT INTO relationship (uidrequestor, uidrequestee , status, detail_value ) VALUES (@id1,@id2,@status,@detail),(@id2,@id1,@status,@detail)";
                    using (var insertCommand = new SQLiteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@id1", requestor);
                        insertCommand.Parameters.AddWithValue("@id2", requestee);
                        insertCommand.Parameters.AddWithValue("@detail", detail);
                        insertCommand.Parameters.AddWithValue("@status", PlayerRelationship.friend);

                        int rowsAffected = insertCommand.ExecuteNonQuery();
                        if (rowsAffected == 0)
                            return FriendDBret.Failed;
                        else
                        {
                            return FriendDBret.Succeeded;
                        }
                    }

                    Log.WriteLine(1, "[RMC FriendInvite] Friendship Accepted", Color.Orange);
                }
                else
                {
                    Log.WriteLine(1, "[RMC FriendInvite] Already Friend", Color.Orange);
                    return FriendDBret.AlreadyPresent;

                }
            }
            return FriendDBret.Failed;
        }

        public static List<RelationshipDBData> ReturnRelationships(uint requestor, byte status, bool reversed)
        {//verify users exist beforehand
            string query;
            if (reversed == true)
            {
                query = "SELECT * FROM relationship WHERE uidrequestee = @requestor AND status = @status ";
            }
            else
            {
                query = "SELECT * FROM relationship WHERE uidrequestor = @requestor AND status = @status  ";
            }

            List<RelationshipDBData> relations = new List<RelationshipDBData>();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@requestor", requestor);
                command.Parameters.AddWithValue("@status", status);
                Log.WriteLine(1, $"[RMC RelationshipList] returned {query} requested ");
                Log.WriteLine(1, "Command Text: " + command.CommandText);

                // Print each parameter and its value
                foreach (SQLiteParameter param in command.Parameters)
                {
                    Log.WriteLine(1, $"{param.ParameterName} = {param.Value}");
                }
                using (var reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        var relationship = new RelationshipDBData
                        {
                            Pidrequestor = Convert.ToUInt32(reader.GetInt32(0)),
                            Pidrequestee = Convert.ToUInt32(reader.GetInt32(1)),
                            Status = reader.GetByte(2),
                            Details = Convert.ToUInt32(reader.GetInt32(3))

                        };
                        relations.Add(relationship);
                    }
                }
            }
            Log.WriteLine(1, $"[RMC RelationshipList] returned {query} requested ");
            return relations;
        }
        public static Boolean RemoveRelationshipBoth(uint requestor, uint requestee, byte status)
        {
            string query = @"
            DELETE FROM relationship
            WHERE ((uidrequestor = @id1 AND uidrequestee = @id2) OR (uidrequestor = @id2 AND uidrequestee = @id1))
              AND status = @status;
            ";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id1", requestor);
                command.Parameters.AddWithValue("@id2", requestee);
                command.Parameters.AddWithValue("@status", status);

                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} relationship(s) removed.");
                return rowsAffected > 0;
            }
            return false;
        }
        public static Boolean RemoveRelationshipOneSided(uint requestor, uint requestee, byte status)
        {
            string query = @"
            DELETE FROM relationship
            WHERE (uidrequestor = @id1 AND uidrequestee = @id2) 
              AND status = @status;
            ";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id1", requestor);
                command.Parameters.AddWithValue("@id2", requestee);
                command.Parameters.AddWithValue("@status", status);

                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"{rowsAffected} relationship(s) removed.");
                return rowsAffected > 0;
            }
        }


        public static User GetUserByName(string name)
        {
            User result = null;
            String query = "SELECT * FROM users WHERE name=@name";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@name", name);
                using (var reader = command.ExecuteReader())

                    while (reader.Read())
                {
                    result = new User
                    {
                        UserDBPid = Convert.ToUInt32(reader.GetInt32(reader.GetOrdinal("pid"))),
                        Name = reader.GetString(reader.GetOrdinal("name")),
                        Password = reader.GetString(reader.GetOrdinal("password")),
                        UserDBUbiId = reader.GetString(reader.GetOrdinal("ubi_id")),
                        Email = reader.GetString(reader.GetOrdinal("email")),
                        CountryCode = reader.GetString(reader.GetOrdinal("country_code")),
                        PrefLang = reader.GetString(reader.GetOrdinal("pref_lang"))
                    };
                        Log.WriteLine(1,$"user is  {result}");
                    return result;
                }
            }
            return result;
        }

        public static User GetUserByID(uint UserDBpid)
        {
            User result = null;
            string query = @"SELECT * FROM users WHERE pid=@id";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", UserDBpid);
                using (var reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        result = new User
                        {
                            UserDBPid = Convert.ToUInt32(reader.GetInt32(reader.GetOrdinal("pid"))),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Password = reader.GetString(reader.GetOrdinal("password")),
                            UserDBUbiId = reader.GetString(reader.GetOrdinal("ubi_id")),
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
        public static List<User> GetUsersByID(List<uint>userids)
        {
            List<User> results = new List<User> { };
            string placeholders = string.Join(", ", userids.Select((_, i) => $"@id{i}"));
            string query = $"SELECT * FROM users WHERE pid IN ({placeholders})";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                for (int i = 0; i < userids.Count; i++)
                {
                    command.Parameters.AddWithValue($"@id{i}", userids[i]);
                }
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Append( new User
                        {
                            UserDBPid = Convert.ToUInt32(reader.GetInt32(reader.GetOrdinal("pid"))),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Password = reader.GetString(reader.GetOrdinal("password")),
                            UserDBUbiId = reader.GetString(reader.GetOrdinal("pid")),
                            Email = reader.GetString(reader.GetOrdinal("email")),
                            CountryCode = reader.GetString(reader.GetOrdinal("country_code")),
                            PrefLang = reader.GetString(reader.GetOrdinal("pref_lang"))
                        });
                        
                    }
                }
            }


            return results;
        }
        public static List<Privilege> GetPrivileges(string locale)
        {
            List<Privilege> privileges = new List<Privilege>();
            List<List<string>> results = GetQueryResults($"SELECT * FROM privileges WHERE locale='{locale}'");
            if (results.Count == 0)
                Log.WriteLine(1, $"[RMC Privileges] Unknown locale: {locale}", Color.Red);

            foreach (List<string> entry in results)
            {
                privileges.Add(new Privilege()
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
                rewards.Add(new UplayReward(platform)
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
