using System;
using System.Collections.Generic;
using System.Text;

namespace PartsUnlimited.Models
{
    public class PaymentTransactionDetails
    {
        public int TransactionId { get; set; }
        public string CustomerTransactionId { get; set; }
        public int TransactionOrderId { get; set; }
        public string CustomerId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionFinalAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; }
    }
}
