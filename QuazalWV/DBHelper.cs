using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;

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

        public static User GetUserByName(string name)
        {
            User result = null;
            List<List<string>> results = GetQueryResults("SELECT * FROM users WHERE name='" + name + "'");
            foreach(List<string> entry in results)
            {
                result = new User
                {
                    Pid = Convert.ToUInt32(entry[1]),
                    Name = name,
                    Password = entry[3],
                    UbiId = entry[4],
                    Email = entry[5],
                    CountryCode = entry[6],
                    PrefLang = entry[7],
                };
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
            foreach(var entry in results)
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
