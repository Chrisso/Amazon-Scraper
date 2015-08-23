using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace Amz.Scrape
{
    public class Scraper : IOrderLoader
    {
        private CookieContainer cookies;
        private System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.GetCultureInfo("de-DE");

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

        public List<Order> LoadYear(int year, string url)
        {
            List<Order> result = new List<Order>();
            List<string> orderPages = new List<string>();
            string prefix = new Uri(string.Format(url, year)).GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped);

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
                else result.AddRange(ScanOrders(doc.DocumentNode.SelectSingleNode("//div[@id='ordersContainer']"), prefix));
            }

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
                    result.AddRange(ScanOrders(doc.DocumentNode.SelectSingleNode("//div[@id='ordersContainer']"), prefix));
                }
            }

            return result;
        }

        private List<Order> ScanOrders(HtmlAgilityPack.HtmlNode node, string prefix)
        {
            List<Order> orders = new List<Order>();

            foreach (HtmlAgilityPack.HtmlNode order in node.SelectNodes(".//div[contains(@class, 'order')]"))
            {
                HtmlAgilityPack.HtmlNode info = order.SelectSingleNode(".//div[contains(@class, 'order-info')]");
                Order o = new Order();

                if (info != null)
                {
                    HtmlAgilityPack.HtmlNode price = info.SelectSingleNode(".//div[contains(@class, 'a-span2')]//span[contains(@class, 'value')]");
                    if (price != null)
                    {
                        o.Sum = ScanPrice(price.InnerText.Trim());
                    }

                    HtmlAgilityPack.HtmlNode id = info.SelectSingleNode(".//div[contains(@class, 'a-col-right')]//span[contains(@class, 'value')]");
                    if (id != null)
                    {
                        o.Id = id.InnerText.Trim();
                    }

                    HtmlAgilityPack.HtmlNode date = info.SelectSingleNode(".//div[contains(@class, 'a-span4')]//span[contains(@class, 'value')]");
                    if (date != null)
                    {
                        o.Date = ScanDate(date.InnerText.Trim());
                    }
                }

                if (o.IsInitialized())
                {
                    foreach (HtmlAgilityPack.HtmlNode product in order.SelectNodes(".//div[contains(@class, 'a-spacing')]//div[contains(@class, 'a-col-right')]"))
                    {
                        HtmlAgilityPack.HtmlNode name = product.SelectSingleNode(".//a[contains(@class, 'a-link-normal')]");
                        HtmlAgilityPack.HtmlNode price = product.SelectSingleNode(".//span[contains(@class, 'a-color-price')]");
                        if ((name != null) && (price != null))
                        {
                            Product p = new Product();
                            p.Price = ScanPrice(price.InnerText.Trim());
                            p.Url = name.Attributes["href"].Value.StartsWith("http") ?
                                        name.Attributes["href"].Value : prefix + name.Attributes["href"].Value;
                            p.Name = WebUtility.HtmlDecode(name.InnerText.Trim());
                            o.Products.Add(p);
                        }
                    }

                    orders.Add(o);
                }
            }

            return orders;
        }

        private double ScanPrice(string s)
        {
            if (s.StartsWith("EUR "))
                return Convert.ToDouble(s.Substring(4), ci.NumberFormat);
            return 0.0;
        }

        private DateTime ScanDate(string s)
        {
            DateTime result = new DateTime();
            DateTime.TryParse(s, ci.DateTimeFormat, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out result);
            return result;
        }
    }
}
