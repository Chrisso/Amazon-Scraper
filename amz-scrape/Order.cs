using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Amz.Scrape
{
    public class Order
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public double Sum { get; set; }
        public List<Product> Products { get; }

        public Order()
        {
            Products = new List<Product>();
        }

        public bool IsInitialized()
        {
            return !string.IsNullOrEmpty(Id) && (Sum != 0.0);
        }
    }

    public class Product
    {
        public string ISIN { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }

        private string _Url;
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                _Url = value;
                Regex regex = new Regex("/product/([^/]*)/");
                Match m = regex.Match(_Url);
                if (m.Success)
                    ISIN = m.Groups[1].Value;
            }
        }
        
    }
}
