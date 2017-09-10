using System;
using System.Linq;
using Amz.Data;

namespace Amz.Scrape
{
    class Program
    {
        static void Main(string[] args)
        {
            bool offline = false;
            for (int i=0; i<args.Length; i++)
            {
                if (string.Compare(args[i], "-help", true) == 0)
                {
                    Console.WriteLine("Options:");
                    Console.WriteLine("\t-clean\tclear local cache");
                    Console.WriteLine("\t-help\tshow this message");
                    Console.WriteLine("\t-o\toffline mode (use cache only)");
                    return;
                }
                if (string.Compare(args[i], "-clean", true) == 0)
                {
                    new Cache().Clean();
                    return;
                }
                if (string.Compare(args[i], "-o", true) == 0)
                    offline = true;
            }

            Cache cache = new Cache();
            IOrderLoader loader = null;

            if (!offline)
            {
                string domain = Properties.Settings.Default.BaseDomain;

                Console.WriteLine("Searching cookies from Google Chrome...");
                System.Net.CookieContainer cookies = new Amz.Auth.CookiesChrome(domain);
                if (cookies.Count == 0)
                {
                    Console.WriteLine("Searching cookies from Mozilla Firefox...");
                    cookies = new Amz.Auth.CookiesFirefox(domain);
                }

                if (cookies.Count > 0)
                {
                    Console.WriteLine("Trying browser login credentials (" + cookies.Count + " cookies)...");
                    loader = new Scraper(cookies);
                }
                else
                {
                    Console.Error.WriteLine("Could not log in!");
                    Console.Error.WriteLine("Hint: use your browser to log in and choose \"remember me\" to get a re-useable auth cookie.");
                }
            }
            else loader = cache;

            if (loader != null)
            {
                try
                {
                    var years = loader.LoadOverview(Properties.Settings.Default.StartUrl);
                    double total = 0.0;

                    foreach (var n in years)
                    {
                        Console.WriteLine("Loading " + n + "...");
                        var orders = loader.LoadYear(n, Properties.Settings.Default.HistoryUrlTemplate);
                        if (orders.Count != cache.Store(orders))
                            loader = cache;

                        double year_total = orders.Aggregate(0.0, (acc, o) => acc + o.Sum);
                        Console.WriteLine("\tTotal: " + year_total);
                        total += year_total;

                        #if DEBUG
                            if (n < DateTime.Now.Year) break;
                        #endif
                    }

                    Console.WriteLine("Total: " + total);
                }
                catch (Exception exc)
                {
                    Console.Error.WriteLine(exc.Message);
                }
            }

            cache.Dispose();

#if DEBUG
            Console.WriteLine("Any key to exit.");
            Console.ReadKey();
#endif
        }
    }
}