using System;
using System.Linq;

namespace Amz.Scrape
{
    class Program
    {
        static void Main(string[] args)
        {
            Cache cache = new Cache();

            Amz.Auth.CookiesFirefox cf = new Auth.CookiesFirefox(Properties.Settings.Default.BaseDomain);
            if (cf.Count > 0)
            {
                Console.WriteLine("Trying Firefox login credentials (" + cf.Count + " cookies)...");
                try
                {
                    Scraper scraper = new Scraper(cf);
                    bool useCache = false;
                    var years = scraper.LoadOverview(Properties.Settings.Default.StartUrl);
                    double total = 0.0;

                    foreach (var n in years)
                    {
                        Console.WriteLine("Scraping " + n + "...");
                        var orders = useCache? cache.LoadYear(n) : scraper.LoadYear(Properties.Settings.Default.HistoryUrlTemplate, n);
                        useCache = orders.Count != cache.Store(orders);

                        double year_total = orders.Aggregate(0.0, (acc, o) => acc + o.Sum);
                        Console.WriteLine("\tTotal: " + year_total);
                        total += year_total;

                        #if DEBUG
                            if (n < 2015) break;
                        #endif
                    }

                    Console.WriteLine("Total: " + total);
                }
                catch (Exception exc)
                {
                    Console.Error.WriteLine(exc.Message);
                }
            }
            else Console.Error.WriteLine("No credentials found!");

            cache.Dispose();

#if DEBUG
            Console.WriteLine("Any key to exit.");
            Console.ReadKey();
#endif
        }
    }
}