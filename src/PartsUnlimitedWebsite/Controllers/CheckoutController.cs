﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PartsUnlimited.Controllers
{
    //[Authorize]
    public class CheckoutController : Controller
    {
        private readonly IPartsUnlimitedContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(IPartsUnlimitedContext context, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _db = context;
        }

        private const string PromoCode = "FREE";

        //
        // GET: /Checkout/

        public async Task<IActionResult> AddressAndPayment()
        {
            //var id = _userManager.GetUserId(User);
            //var user = await _db.Users.FirstOrDefaultAsync(o => o.Id == id);
            //user.Name = "John";
            //user.Email = "john@mapy.com";
            //user.UserName = "john@mapy.com";
            //var Phone = "314-612-3604";
            //var Address = "8926 Johnson Parkway";
            //var City = "Saint Louis";
            //var State = "Missouri";
            //var Country = "United States";
            //var PostalCode = "63101";
            //var order = new Order
            //{
            //    Name = user.Name,
            //    Email = user.Email,
            //    Phone = Phone,
            //    Username = user.UserName,
            //    Address = Address,
            //    City = City,
            //    State = State,
            //    PostalCode = PostalCode,
            //    Country = Country
            //};

            return View();
        }

        //
        // POST: /Checkout/AddressAndPayment

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> AddressAndPayment(Order order,string userid)
        {
            decimal orderTotal = 0;
            var formCollection = await HttpContext.Request.ReadFormAsync();

            try
            {
                if (string.Equals(formCollection["PromoCode"].FirstOrDefault(), PromoCode,
                    StringComparison.OrdinalIgnoreCase) == false)
                {
                    return View(order);
                }
                else
                {
                    //order.Username = HttpContext.User.Identity.Name;
                    order.OrderDate = DateTime.Now;
                    order.Username = "Safi";
                    //order.FinalValue = finalprice;

                    //Add the Order
                    _db.Orders.Add(order);

                    //Process the order
                    //var cart = ShoppingCart.GetCart(_db, HttpContext);
                    //cart.CreateOrder(order);
                    var cart = ShoppingCart.GetCartDemo(_db, HttpContext, userid);
                    foreach (var item in cart)
                    {
                        //var product = _db.Products.Find(item.ProductId);
                        var product = _db.Products.Single(a => a.ProductId == item.ProductId);

                        var orderDetail = new OrderDetail
                        {
                            ProductId = item.ProductId,
                            OrderId = order.OrderId,
                            UnitPrice = product.Price,
                            Quantity = item.Count,
                        };

                        // Set the order total of the shopping cart
                        orderTotal += (item.Count * product.Price);

                        _db.OrderDetails.Add(orderDetail);
                    }
                    // Save all changes
                    await _db.SaveChangesAsync(HttpContext.RequestAborted);

                    return RedirectToAction("Complete",
                        new { id = order.OrderId });
                }
            }
            catch
            {
                //Invalid - redisplay with errors
                return View(order);
            }
        }

        //
        // GET: /Checkout/Complete

        public IActionResult Complete(int id)
        {
            // Validate customer owns this order
            //Order order = _db.Orders.FirstOrDefault(
            //    o => o.OrderId == id &&
            //    o.Username == HttpContext.User.Identity.Name);

            Order order = _db.Orders.FirstOrDefault(
              o => o.OrderId == id);

            if (order != null)
            {
                return View(order);
            }
            else
            {
                return View("Error");
            }
        }
    }
}
