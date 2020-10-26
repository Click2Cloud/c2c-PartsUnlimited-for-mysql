using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PartsUnlimited.Models
{
    public class CartItemByUser
    {
        [Key]
        public int CartItemId { get; set; }
        public string CartId { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual Product Product { get; set; }
        public string UserID { get; set; }
        public string Email { get; set; }
    }
}
