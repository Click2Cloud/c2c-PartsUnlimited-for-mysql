using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace PartsUnlimited.Models
{
    public class PaymentDetails
    {
        public int PaymentDetailsID { get; set; }
        public int OrderId { get; set; }
        public string Username { get; set; }
        public string ProductDetails { get; set; }
        public decimal Price { get; set; }
        public string Title { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public Dictionary<string, string> ProductDetailList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ProductDetails))
                {
                    return new Dictionary<string, string>();
                }
                try
                {
                    var obj = JToken.Parse(ProductDetails);
                }
                catch (Exception)
                {
                    throw new FormatException("Product Details only accepts json format.");
                }
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(ProductDetails);
            }
        }
    }
}
