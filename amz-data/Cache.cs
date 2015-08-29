using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;

namespace Amz.Data
{
    public class Cache : IOrderLoader, IDisposable
    {
        private SQLiteConnection connection;

        public Cache(string name="AmazonOrders.sqlite")
        {
            string db = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), name);

            connection = new SQLiteConnection("Data Source=" + db);
            connection.Open();

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS [Orders] ([Id] INTEGER PRIMARY KEY, [AId] TEXT NOT NULL UNIQUE, [Total] REAL NOT NULL, [Date] DATETIME NOT NULL)";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE IF NOT EXISTS [Products] ([Id] INTEGER PRIMARY KEY, [Order] INTEGER NOT NULL, [ASIN] TEXT NOT NULL, [Name] TEXT NOT NULL, [Price] REAL NOT NULL, [Url] TEXT NOT NULL)";
            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            connection.Close();
        }

        public void Clean()
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "DROP TABLE IF EXISTS [Products]";
            command.ExecuteNonQuery();
            command.CommandText = "DROP TABLE IF EXISTS [Orders]";
            command.ExecuteNonQuery();

            string file = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                connection.DataSource + ".sqlite"); // assumes extension!

            connection.Close();
            connection.Dispose();
            connection = null;

            if (File.Exists(file))
            {
                try { File.Delete(file); }
                catch (IOException) { /* nothing */ }
            }
        }

        public int Store(List<Order> orders)
        {
            int inserted = 0;

            SQLiteCommand insertCheck = connection.CreateCommand();
            insertCheck.CommandText = "SELECT [Id] FROM [Orders] WHERE [AId] = ?";
            insertCheck.Parameters.Add("aid", DbType.String);
            insertCheck.Prepare();

            SQLiteCommand insertOrder = connection.CreateCommand();
            insertOrder.CommandText = "INSERT INTO [Orders] ([AId], [Total], [Date]) VALUES (?, ?, ?)";
            insertOrder.Parameters.Add("aid", DbType.String);
            insertOrder.Parameters.Add("ttl", DbType.Double);
            insertOrder.Parameters.Add("dat", DbType.DateTime);
            insertOrder.Prepare();

            SQLiteCommand insertProduct = connection.CreateCommand();
            insertProduct.CommandText = "INSERT INTO [Products] ([Order], [ASIN], [Name], [Price], [Url]) VALUES (?, ?, ?, ?, ?)";
            insertProduct.Parameters.Add("oid", DbType.Int32);
            insertProduct.Parameters.Add("asin", DbType.String);
            insertProduct.Parameters.Add("name", DbType.String);
            insertProduct.Parameters.Add("pric", DbType.Double);
            insertProduct.Parameters.Add("url", DbType.String);
            insertProduct.Prepare();

            foreach (Order order in orders)
            {
                insertCheck.Parameters[0].Value = order.Id;
                if (insertCheck.ExecuteScalar() == null)
                {
                    insertOrder.Parameters[0].Value = order.Id;
                    insertOrder.Parameters[1].Value = order.Sum;
                    insertOrder.Parameters[2].Value = order.Date;
                    insertOrder.ExecuteNonQuery();

                    insertProduct.Parameters[0].Value = (int)(System.Int64)insertCheck.ExecuteScalar();
                    foreach (Product product in order.Products)
                    {
                        insertProduct.Parameters[1].Value = product.ASIN;
                        insertProduct.Parameters[2].Value = product.Name;
                        insertProduct.Parameters[3].Value = product.Price;
                        insertProduct.Parameters[4].Value = product.Url;
                        insertProduct.ExecuteNonQuery();
                    }

                    inserted++;
                }
            }

            return inserted;
        }

        public List<int> LoadOverview(string unused)
        {
            List<int> result = new List<int>();

            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT DISTINCT strftime('%Y', [Date]) FROM [Orders]";

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                    result.Add(Convert.ToInt32(reader.GetString(0)));
            }

            return result;
        }

        public List<Order> LoadYear(int year, string unused)
        {
            List<Order> result = new List<Order>();

            SQLiteCommand selectOrders = connection.CreateCommand();
            selectOrders.CommandText = "SELECT [Id], [AId], [Total], [Date] FROM [ORDERS] WHERE strftime('%Y', [Date])=? ORDER BY [Date] DESC";
            selectOrders.Parameters.Add("year", DbType.String).Value = year.ToString();
            selectOrders.Prepare();

            using (SQLiteDataReader reader = selectOrders.ExecuteReader())
            {
                while (reader.Read())
                {
                    Order o = new Order();
                    o.Id = reader.GetString(1);
                    o.Sum = reader.GetDouble(2);
                    o.Date = reader.GetDateTime(3);
                    o.Products.AddRange(LoadProducts(reader.GetInt32(0)));
                    result.Add(o);
                }
            }

            return result;
        }

        private List<Product> LoadProducts(int oid)
        {
            List<Product> result = new List<Product>();

            SQLiteCommand selectProducts = connection.CreateCommand();
            selectProducts.CommandText = "SELECT [ASIN], [Name], [Price], [Url] FROM [Products] WHERE [Order]=?";
            selectProducts.Parameters.Add("oid", DbType.Int32).Value = oid;
            selectProducts.Prepare();

            using (SQLiteDataReader reader = selectProducts.ExecuteReader())
            {
                while (reader.Read())
                {
                    Product p = new Product();
                    p.ASIN = reader.GetString(0);
                    p.Name = reader.GetString(1);
                    p.Price = reader.GetDouble(2);
                    p.Url = reader.GetString(3);
                    result.Add(p);
                }
            }

            return result;
        }
    }
}
