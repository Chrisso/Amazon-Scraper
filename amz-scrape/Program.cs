using System;

namespace Amz.Scrape
{
    class Program
    {
        static void Main(string[] args)
        {
            Amz.Auth.CookiesFirefox cf = new Auth.CookiesFirefox(Properties.Settings.Default.BaseDomain);
            Console.WriteLine("# of cookies loaded: " + cf.Count);

#if DEBUG
            Console.WriteLine("Any key to exit.");
            Console.ReadKey();
#endif
        }
    }
}