using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

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
    }
}
