using System;

namespace Amz.Scrape
{
    public class Order
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public double Sum { get; set; }

        public bool IsInitialized()
        {
            return !string.IsNullOrEmpty(Id) && (Sum != 0.0);
        }
    }
}
