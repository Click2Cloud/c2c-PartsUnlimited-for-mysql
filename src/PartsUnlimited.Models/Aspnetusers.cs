using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartsUnlimited.Models
{
    public class Aspnetusers
    {
       
        public string Id { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string Email { get; set; }

        public string NormalizedEmail { get; set; }
        [Column(TypeName = "TINYINT(1)")]
        public bool EmailConfirmed { get; set; }

        public string Password { get; set; }

        public string SecurityStamp { get; set; }

        public string ConcurrencyStamp { get; set; }

        public string PhoneNumber { get; set; }

        public int AccessFailedCount { get; set; }

        [Column(TypeName = "TINYINT(1)")]
        public bool PhoneNumberConfirmed { get; set; }

        [Column(TypeName = "TINYINT(1)")]
        public bool TwoFactorEnabled { get; set; }

        [Column(TypeName = "TINYINT(1)")]
        public bool LockoutEnd { get; set; }

        [Column(TypeName = "TINYINT(1)")]
        public bool LockoutEnabled { get; set; }

        public string Name { get; set; }

    }
}
