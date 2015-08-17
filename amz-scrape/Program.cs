using System;
using System.IO;
using System.Net;

namespace Amz.Scrape
{
    class Program
    {
        static void Main(string[] args)
        {
            Amz.Auth.CookiesFirefox cf = new Auth.CookiesFirefox(Properties.Settings.Default.BaseDomain);
            if (cf.Count > 0)
            {
                Console.WriteLine("Trying Firefox login credentials (" + cf.Count + " cookies)...");
                try
                {
                    Scraper scraper = new Scraper(cf);
                    var years = scraper.LoadOverview(Properties.Settings.Default.StartUrl);
                    double total = 0.0;

                    foreach (var n in years)
                    {
                        Console.WriteLine("Scraping " + n + "...");
                        total += scraper.LoadYear(Properties.Settings.Default.HistoryUrlTemplate, n);

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

#if DEBUG
            Console.WriteLine("Any key to exit.");
            Console.ReadKey();
#endif
        }
    }
}