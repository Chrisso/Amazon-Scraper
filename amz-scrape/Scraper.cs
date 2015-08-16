using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace Amz.Scrape
{
    public class Scraper
    {
        private CookieContainer cookies;

        public Scraper(CookieContainer cc)
        {
            cookies = cc;
        }

        public List<int> LoadOverview(string url)
        {
            List<int> result = new List<int>();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = cookies;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            cookies.Add(response.Cookies); // for further requests

            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(sr.ReadToEnd());

                HtmlAgilityPack.HtmlNode node = doc.DocumentNode.SelectSingleNode("//select[@name='orderFilter']");
                if (node != null)
                {
                    foreach (HtmlAgilityPack.HtmlNode option in node.SelectNodes("option[@value]"))
                    {
                        string year = option.Attributes["value"].Value;
                        if (year.StartsWith("year-"))
                        {
                            int n = 0;
                            if (int.TryParse(year.Substring(5), out n))
                                result.Add(n);
                        }
                    }
                }
                else throw new InvalidOperationException("Login failed!");
            }

            return result;
        }
    }
}
