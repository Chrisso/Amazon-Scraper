﻿using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Amz.Data
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

    public interface IOrderLoader
    {
        List<int> LoadOverview(string url);
        List<Order> LoadYear(int year, string url);
    }

    public class Product
    {
        public string ASIN { get; set; }
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
                    ASIN = m.Groups[1].Value;
            }
        }
        
    }
}
