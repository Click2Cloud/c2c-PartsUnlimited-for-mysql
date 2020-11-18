using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; }
    }
}
