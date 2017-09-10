using System;
using System.IO;
using System.Net;
using System.Data;
using System.Data.SQLite;

namespace Amz.Auth
{
    public class CookiesChrome : CookieContainer
    {
        public CookiesChrome(string domain)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google", "Chrome", "User Data", "Default", "Cookies");
            if (File.Exists(path))
                LoadCookies(path, domain);
        }

        private void LoadCookies(string file, string domain)
        {
            SQLiteConnection conn = new SQLiteConnection("Data Source=" + file);
            try
            {
                conn.Open();

                SQLiteCommand command = new SQLiteCommand();
                command.Connection = conn;
                command.CommandText = "SELECT [name], [value], [path], [expires_utc], [encrypted_value] FROM [cookies] WHERE [host_key] LIKE ?";
                command.Parameters.Add("domain", DbType.String).Value = "%" + domain;

                DateTime epoch = new DateTime(1601, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                DateTime now = DateTime.Now;

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime dt = epoch.AddMilliseconds(reader.GetInt64(3) / 1000).ToLocalTime();
                        if (now < dt)
                        {
                            byte[] blob = (byte[])reader[4];
                            string value = reader.GetString(1);
                            if (blob.Length != 0)
                            {
                                byte[] decoded = System.Security.Cryptography.ProtectedData.Unprotect(
                                    blob, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                                value = System.Text.Encoding.ASCII.GetString(decoded);
                            }
                            Cookie cookie = new Cookie(reader.GetString(0), value, reader.GetString(2), domain);
                            Add(cookie);
                        }
                    }
                }
            }
            catch (SQLiteException)
            {

            }
            finally
            {
                conn.Close();
            }
        }
    }
}
