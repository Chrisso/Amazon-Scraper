using System;
using System.IO;
using System.Net;
using System.Data;
using System.Data.SQLite;

namespace Amz.Auth
{
    public class CookiesFirefox : CookieContainer
    {
        public CookiesFirefox(string domain)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Mozilla", "Firefox", "Profiles");
            string[] profiles = Directory.GetDirectories(path, "*.default");
            if (profiles.Length > 0)
            {
                path = Path.Combine(profiles[0], "cookies.sqlite");
                if (File.Exists(path))
                    LoadCookies(path, domain);
            }
        }

        private void LoadCookies(string file, string domain)
        {
            SQLiteConnection conn = new SQLiteConnection("Data Source=" + file);
            try
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand();
                command.Connection = conn;
                command.CommandText = "SELECT [name], [value], [path], [expiry] FROM [moz_cookies] WHERE [baseDomain] LIKE ?";
                command.Parameters.Add("domain", DbType.String).Value = domain;

                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                DateTime now = DateTime.Now;

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime dt = epoch.AddSeconds(reader.GetInt64(3)).ToLocalTime();
                        if (now < dt)
                        {
                            Add(new Cookie(reader.GetString(0), reader.GetString(1), reader.GetString(2), domain));
                        }
                    }
                }
            }
            catch (SQLiteException exc)
            {
                Console.WriteLine("Warning: " + exc.Message);
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
