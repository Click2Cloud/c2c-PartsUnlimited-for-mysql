// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
//using MySQL.Data.EntityFrameworkCore.Extension
using MySql.Data.EntityFrameworkCore.Extensions;


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
        public DbSet<Aspnetusers> Aspnetusers { get; set; }
        public DbSet<CartItemByUser> CartItemsByUser { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Product>().Ignore(a => a.ProductDetailList).HasKey(a => a.ProductId);
            builder.Entity<Order>().HasKey(o => o.OrderId);
            builder.Entity<Category>().HasKey(g => g.CategoryId);
            builder.Entity<CartItem>().HasKey(c => c.CartItemId);
            builder.Entity<OrderDetail>().HasKey(o => o.OrderDetailId);
            builder.Entity<Raincheck>().HasKey(o => o.RaincheckId);
            builder.Entity<Store>().HasKey(o => o.StoreId);
            builder.Entity<CartItemByUser>().HasKey(ci => ci.CartItemId);
            //builder.Entity<Aspnetusers>().HasKey(a => a.Id);
            //builder.Entity<Aspnetusers>().Property(a => a.AccessFailedCount).HasConversion<int>();

            //builder.Entity<Aspnetusers>(entity =>
            //{
            //    entity.Property(m => m.EmailConfirmed).HasConversion<int>();
            //    entity.Property(m => m.PhoneNumberConfirmed).HasConversion<int>();
            //    entity.Property(m => m.TwoFactorEnabled).HasConversion<int>();
            //    entity.Property(m => m.LockoutEnd).HasConversion<int>();
            //    entity.Property(m => m.LockoutEnabled).HasConversion<int>();
            //    entity.Property(m => m.AccessFailedCount).HasConversion<int>();
            //});




            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                //optionsBuilder.UseSqlServer(_connectionString);
                optionsBuilder.UseMySQL(_connectionString);
            }
            else
            {
                System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder(_connectionString);
                optionsBuilder.UseInMemoryDatabase("Test");                
                
            }
            
        }
        
    }
}