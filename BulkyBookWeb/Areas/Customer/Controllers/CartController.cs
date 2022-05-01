﻿using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        readonly IUnitOfWork _db;

        public CartController(IUnitOfWork db)
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
                CartTotal = total
            };

            return View(cart);
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
