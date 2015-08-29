using System;
using Amz.Data;

namespace Amz.Scrape
{
    class Program
    {
        static void Main(string[] args)
        {
            Cache cache = new Cache();
            System.Globalization.NumberFormatInfo nfi = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

            foreach (int year in cache.LoadOverview(null))
            {
                foreach (Order order in cache.LoadYear(year, null))
                {
                    foreach (Product product in order.Products)
                    {
                        Console.Write(order.Id + "\t");
                        Console.Write(order.Date.ToShortDateString() + "\t");
                        Console.Write(product.Name + "\t");
                        Console.Write(product.ASIN + "\t");
                        Console.Write(Convert.ToString(product.Price, nfi) + "\t");
                        Console.Write(product.Url + "\t");
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
