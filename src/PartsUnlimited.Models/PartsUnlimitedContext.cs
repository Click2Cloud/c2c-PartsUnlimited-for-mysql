// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;

namespace PartsUnlimited.Models
{
    public class PartsUnlimitedContext : IdentityDbContext<ApplicationUser>, IPartsUnlimitedContext
    {
        
        private readonly string _connectionString;        

        public PartsUnlimitedContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Raincheck> RainChecks { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<PaymentDetails> PaymentDetails { get; set; }
        public DbSet<PaymentTransactionDetails> PaymentTransactionDetails { get; set; }
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Product>().Ignore(a => a.ProductDetailList).HasKey(a => a.ProductId);
            builder.Entity<Order>().HasKey(o => o.OrderId);
            builder.Entity<Category>().HasKey(g => g.CategoryId);
            builder.Entity<CartItem>().HasKey(c => c.CartItemId);
            builder.Entity<OrderDetail>().HasKey(o => o.OrderDetailId);
            builder.Entity<Raincheck>().HasKey(o => o.RaincheckId);
            builder.Entity<Store>().HasKey(o => o.StoreId);
            builder.Entity<PaymentDetails>().HasKey(p => p.PaymentDetailsID);
            builder.Entity<PaymentTransactionDetails>().HasKey(t => t.TransactionId);
            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                // optionsBuilder.UseSqlServer(_connectionString);
                optionsBuilder.UseMySql(_connectionString,b=>b.MigrationsAssembly("PartsUnlimitedWebsite"));
                //optionsBuilder.UseMySql("server="+ DBServerIP+";User Id="+ DBUserId + ";password="+ DBPassword + ";database="+ DBName + ";persistsecurityinfo="+ DBSecurityInfo + ";", b => b.MigrationsAssembly("PartsUnlimitedWebsite"));

            }
            else
            {
                //System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder(_connectionString);
                MySql.Data.MySqlClient.MySqlConnectionStringBuilder builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(_connectionString);
                optionsBuilder.UseInMemoryDatabase("Test");
            }
        }
    }
}