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

        //payment code
        public IActionResult Payment()
        {
            return View();
        }
        public async Task<IActionResult> Charge(string stripeEmail, string stripeToken)
        {
            string LoginEmailId=HttpContext.Session.GetString("LoginEmail");
            ApplicationUser applicationUser=_db.Users.Where(m => m.Email == LoginEmailId).Single();            
            string SubTotalValue = "";
            decimal SubTotal = 0.00M;
            long PayAmount = 0;
            decimal shipping = 0.00M;
            decimal tax = 0.00M;
            int ItemCount =Convert.ToInt32(HttpContext.Session.GetString("CartItemCount"));
            int OrderID =Convert.ToInt32(TempData["OrderID"]);
            var getCustomerDetails = _db.Orders.Where(o => o.OrderId == OrderID).ToList();
            var orderDetails = _db.OrderDetails.Where(od => od.OrderId == OrderID).ToList();
            SubTotal = getCustomerDetails[0].Total;
            foreach (var item in orderDetails)
            {
                var product = _db.Products.Single(a => a.ProductId == item.ProductId);
                var paymentOrderDetails = new PaymentDetails
                {
                    OrderId = item.OrderId,
                    Username = getCustomerDetails[0].Name,
                    ProductDetails = item.Product.ProductDetails,
                    Price = item.UnitPrice,
                    Title = item.Product.Title,
                    Quantity = item.Quantity,
                    TotalPrice=item.Quantity*item.UnitPrice,
                };

                shipping= ItemCount * (decimal)5.00;
                tax = (SubTotal +shipping) * (decimal)0.05;

                SubTotalValue = HttpContext.Session.GetString("FinalAmount");
                PayAmount = long.Parse(SubTotalValue.ToString().Replace(".",string.Empty));
                _db.PaymentDetails.Add(paymentOrderDetails);
                await _db.SaveChangesAsync(HttpContext.RequestAborted);
            }
           
            var paymentTransactionDetails = new PaymentTransactionDetails
            {
                CustomerTransactionId = Guid.NewGuid().ToString(),
                TransactionOrderId =OrderID,
                CustomerId= applicationUser.Id,
                TransactionDate=DateTime.Now,
                TransactionFinalAmount= SubTotalValue,
                CustomerName=getCustomerDetails[0].Name,
                ShippingAmount=shipping,
                TaxAmount=tax,
                TotalAmount=SubTotal,
            };

            _db.PaymentTransactionDetails.Add(paymentTransactionDetails);
            await _db.SaveChangesAsync(HttpContext.RequestAborted);



            var options = new PaymentIntentCreateOptions
            {               
                Shipping = new ChargeShippingOptions
                {
                    Name = getCustomerDetails[0].Name,                    
                    Address = new AddressOptions
                    {
                        Line1 = getCustomerDetails[0].Address,
                        PostalCode = getCustomerDetails[0].PostalCode,
                        City = getCustomerDetails[0].City,
                        State = getCustomerDetails[0].State,
                        Country = getCustomerDetails[0].Country,
                    },
                },

                Amount = PayAmount,//050,
                Currency = "usd"

            };

            var service = new PaymentIntentService();
            var intent = service.Create(options);
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
