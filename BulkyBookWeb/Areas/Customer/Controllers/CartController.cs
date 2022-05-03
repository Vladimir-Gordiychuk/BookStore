using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        readonly IUnitOfWork _db;

        public CartController(IUnitOfWork db, IOptions<StripeKeys> options)
        {
            _db = db;
        }
        
        public IActionResult Index()
        {
            var userId = GetCurrentUserId();

            var items = _db.ShoppingCart
                    .Where(
                        item => item.ApplicationUserId == userId,
                        nameof(ShoppingCart.Product))
                    .ToList();

            var total = 0.0;
            foreach (var item in items)
            {
                item.Price = GetPriceBasedOnQuantity(item.Product, item.Count);
                total += item.Price * item.Count;
            }

            var cart = new ShoppingCartVm
            {
                Items = items,
                OrderHeader = new OrderHeader {
                    OrderTotal = total
                }
            };

            return View(cart);
        }

        public IActionResult Summary()
        {
            var userId = GetCurrentUserId();

            var user = _db.ApplicationUser.Find(userId);

            var items = _db.ShoppingCart
                    .Where(
                        item => item.ApplicationUserId == userId,
                        nameof(ShoppingCart.Product))
                    .ToList();

            var total = 0.0;
            foreach (var item in items)
            {
                item.Price = GetPriceBasedOnQuantity(item.Product, item.Count);
                total += item.Price * item.Count;
            }

            var order = new OrderHeader
            {
                ApplicationUserId = userId,
                ApplicationUser = user,
                Name = user.Name,
                StreetAddress = user.StreetAddress,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                PhoneNumber = user.PhoneNumber,
                OrderTotal = total,
            };

            var cart = new ShoppingCartVm
            {
                Items = items,
                OrderHeader = order
            };

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Summary(OrderHeader orderHeader)
        {
            var userId = GetCurrentUserId();

            var user = _db.ApplicationUser.Find(userId);

            var items = _db.ShoppingCart
                    .Where(
                        item => item.ApplicationUserId == userId,
                        nameof(ShoppingCart.Product))
                    .ToList();

            var total = 0.0;
            foreach (var item in items)
            {
                item.Price = GetPriceBasedOnQuantity(item.Product, item.Count);
                total += item.Price * item.Count;
            }

            orderHeader.OrderStatus = SD.StatusPending;
            orderHeader.PaymentStatus = SD.PaymentStatusPending;
            orderHeader.OrderDate = DateTime.Now;
            orderHeader.ApplicationUserId = userId;
            orderHeader.ApplicationUser = null;

            _db.OrderHeader.Add(orderHeader);

            // the following operation is performed
            // in order to get Id of the newly created record.
            // ¯\_(ツ)_/¯ todo: add safe transactions
            _db.Save(); 

            foreach (var item in items)
            {
                _db.OrderDetail.Add(new OrderDetail
                {
                    Price = item.Price,
                    Product = item.Product,
                    Count = item.Count,
                    ProductId = item.ProductId,
                    OrderId = orderHeader.Id
                });
            }

            var domain = "https://localhost:44306/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = items.Select(item =>
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100.0),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title,
                            },
                        },
                        Quantity = item.Count,
                    }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={orderHeader.Id}",
                CancelUrl = domain + $"Customer/Cart/Index",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            orderHeader.SessionId = session.Id;
            orderHeader.PaymentIntentId = session.PaymentIntentId;

            _db.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);


        }

        public IActionResult OrderConfirmation(int id)
        {
            var order = _db.OrderHeader.Find(id);

            var service = new SessionService();
            Session session = service.Get(order.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                order.OrderStatus = SD.StatusApproved;
                order.PaymentStatus = SD.PaymentStatusApproved;
                _db.Save();
            }

            var userId = GetCurrentUserId();
            var items = _db.ShoppingCart.Where(item => item.ApplicationUserId == userId);
            _db.ShoppingCart.RemoveRange(items);

            _db.Save();

            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            var targetCart = _db.ShoppingCart.Find(cartId);
            if (targetCart == null)
                return NotFound();

            targetCart.Count += 1;

            _db.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var targetCart = _db.ShoppingCart.Find(cartId);
            if (targetCart == null)
                return NotFound();

            if (targetCart.Count > 1)
            {
                targetCart.Count -= 1;
            }
            else
            {
                _db.ShoppingCart.Remove(targetCart);
            }

            _db.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var targetCart = _db.ShoppingCart.Find(cartId);
            if (targetCart == null)
                return NotFound();

            _db.ShoppingCart.Remove(targetCart);
 
            _db.Save();

            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(Product product, int count)
        {
            if (count < 50)
                return product.Price;
            if (count < 100)
                return product.Price50;
            return product.Price100;
        }

        private string GetCurrentUserId()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            Debug.Assert(claimsIdentity != null, "User is required to be logged in.");
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            Debug.Assert(claim != null, "All valid users are supposed to have an Id.");
            return claim.Value;
        }


    }
}
