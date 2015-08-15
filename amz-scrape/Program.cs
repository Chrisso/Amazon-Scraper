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
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Properties.Settings.Default.StartUrl);
                request.CookieContainer = cf;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                cf.Add(response.Cookies); // for further requests

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(sr.ReadToEnd());

                    HtmlAgilityPack.HtmlNode node = doc.DocumentNode.SelectSingleNode("//select[@name='orderFilter']");
                    if (node != null)
                    {
                        Console.WriteLine("Available history:");
                        foreach (HtmlAgilityPack.HtmlNode option in node.SelectNodes("option[@value]"))
                        {
                            string year = option.Attributes["value"].Value;
                            if (year.StartsWith("year-"))
                                Console.Write(year.Substring(5) + " ");
                        }
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.Error.WriteLine("Login failed!");
                    }
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