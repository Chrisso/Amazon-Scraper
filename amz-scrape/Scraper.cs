﻿using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace Amz.Scrape
{
    public class Scraper
    {
        private CookieContainer cookies;
        private System.Globalization.NumberFormatInfo nfi = System.Globalization.CultureInfo.GetCultureInfo("de-DE").NumberFormat;

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

        public double LoadYear(string url, int year)
        {
            double orderSum = 0.0;
            List<string> orderPages = new List<string>();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(url, year));
            request.CookieContainer = cookies;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            cookies.Add(response.Cookies); // for further requests

            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(sr.ReadToEnd());

                HtmlAgilityPack.HtmlNode node = doc.DocumentNode.SelectSingleNode("//ul[@class='a-pagination']");
                if (node != null)
                {
                    foreach (var link in node.SelectNodes(".//a[@href]"))
                        orderPages.Add(link.Attributes["href"].Value.Trim());
                    if (orderPages.Count > 1)
                        orderPages.RemoveAt(orderPages.Count - 1); // last link in list is next button
                }
                else orderSum += ScanOrders(doc.DocumentNode.SelectSingleNode("//div[@id='ordersContainer']"));
            }

            string prefix = new Uri(string.Format(url, year)).GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped);
            for (int i=0; i<orderPages.Count; i++)
            {
                Console.WriteLine("\tpage {0}...", i+1);
                string page_url = orderPages[i].StartsWith("http") ? orderPages[i] : prefix + orderPages[i];
                request = (HttpWebRequest)WebRequest.Create(page_url);
                request.CookieContainer = cookies;

                response = (HttpWebResponse)request.GetResponse();
                cookies.Add(response.Cookies); // for further requests

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(sr.ReadToEnd());
                    orderSum += ScanOrders(doc.DocumentNode.SelectSingleNode("//div[@id='ordersContainer']"));
                }
            }

            Console.WriteLine("\tyear total: " + orderSum);
            return orderSum;
        }

        private double ScanOrders(HtmlAgilityPack.HtmlNode node)
        {
            double sum = 0.0;

            foreach (HtmlAgilityPack.HtmlNode order in node.SelectNodes(".//div[contains(@class, 'order')]"))
            {
                HtmlAgilityPack.HtmlNode info = order.SelectSingleNode(".//div[contains(@class, 'order-info')]");
                if (info != null)
                {
                    Console.Write("\tOrder: ");
                    HtmlAgilityPack.HtmlNode price = info.SelectSingleNode(".//div[contains(@class, 'a-span2')]//span[contains(@class, 'value')]");
                    if (price != null)
                    {
                        double p = ScanPrice(price.InnerText.Trim());
                        sum += p;
                        Console.WriteLine(p);
                    }
                    else Console.WriteLine("not found!");
                }
            }

            return sum;
        }

        private double ScanPrice(string s)
        {
            if (s.StartsWith("EUR "))
                return Convert.ToDouble(s.Substring(4), nfi);
            return 0.0;
        }
    }
}