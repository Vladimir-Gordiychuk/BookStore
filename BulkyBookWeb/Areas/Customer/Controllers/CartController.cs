using BulkyBook.DataAccess.Repository;
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

            var cart = new ShoppingCartVm
            {
                Items = _db.ShoppingCart
                    .Where(
                        item => item.ApplicationUserId == userId,
                        nameof(ShoppingCart.Product))
                    .ToList()
            };

            return View(cart);
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
