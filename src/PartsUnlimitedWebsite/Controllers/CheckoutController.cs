// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PartsUnlimited.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Order = PartsUnlimited.Models.Order;

namespace PartsUnlimited.Controllers
{
    [Authorize]
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
            var id = _userManager.GetUserId(User);
            var user = await _db.Users.FirstOrDefaultAsync(o => o.Id == id);
            //user.Name = user.Name; 
            user.Email = user.UserName;  
            //user.UserName = user.UserName;         
            var order = new Order
            {
                Name = user.Name,
                Email = user.Email,               
            };

            return View(order);
        }

        //
        // POST: /Checkout/AddressAndPayment

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddressAndPayment(Order order, string finalamount)
        {
            TempData["FinalPayment"] = finalamount;
            string temp = Regex.Replace(finalamount, "[$]", "");
            HttpContext.Session.SetString("FinalAmount", temp);
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
                    order.Username = HttpContext.User.Identity.Name;
                    order.OrderDate = DateTime.Now;

                    //Add the Order
                    _db.Orders.Add(order);

                    //Process the order
                    var cart = ShoppingCart.GetCart(_db, HttpContext);
                    cart.CreateOrder(order);

                    // Save all changes
                    await _db.SaveChangesAsync(HttpContext.RequestAborted);

                    TempData["OrderID"] = order.OrderId;
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
            Order order = _db.Orders.FirstOrDefault(
                o => o.OrderId == id &&
                o.Username == HttpContext.User.Identity.Name);

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
