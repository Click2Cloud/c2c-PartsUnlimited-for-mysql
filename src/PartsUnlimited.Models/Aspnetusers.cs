using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartsUnlimited.Models
{
    public class Aspnetusers
    {
       
        public string Id { get; set; }
        [Required]
        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        public string NormalizedEmail { get; set; }
        [Column(TypeName = "TINYINT(1)")]
        public bool EmailConfirmed { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]

        public string ConfirmPassword { get; set; }

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
