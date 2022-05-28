using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        readonly string CategoryAndCoverType = string.Join(",", nameof(Product.Category), nameof(Product.CoverType));

        readonly ILogger<HomeController> _logger;
        readonly IUnitOfWork _db;


        public HomeController(ILogger<HomeController> logger, IUnitOfWork db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            var products = _db.Product
                .GetAll(includeProperties: CategoryAndCoverType)
                .ToList();

            return View(products);
        }

        [HttpGet]
        public IActionResult Details(int productId)
        {
            var targetProduct = _db.Product.Find(productId);
            if (targetProduct is null)
                return NotFound();

            ShoppingCart cart = GetShoppingCart(productId);

            return View(cart);
        }

        private ShoppingCart GetShoppingCart(int productId)
        {
            var targetProduct = _db.Product.GetFirstOrDefault(
                item => item.Id == productId,
                string.Join(",", nameof(Product.CoverType), nameof(Product.Category)));

            var cart = new ShoppingCart()
            {
                ProductId = productId,
                Product = targetProduct,
                Count = 1
            };
            return cart;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            cart.ApplicationUserId = GetCurrentUserId();
            cart.ApplicationUser = null;

            //if (!ModelState.IsValid)
            //{
            //    return View(GetShoppingCart(cart.Product.Id));
            //}

            var tarterCart = _db.ShoppingCart.GetFirstOrDefault(
                record =>
                    record.ApplicationUserId == cart.ApplicationUserId &&
                    record.ProductId == cart.ProductId);

            if (tarterCart == null)
            {
                cart.ProductId = cart.Product.Id;
                cart.Product = null;

                _db.ShoppingCart.Add(cart);
            }
            else
            {
                tarterCart.Count += cart.Count;
            }

            _db.Save();

            var itemCount = _db.ShoppingCart
                .Where(record => record.ApplicationUserId == cart.ApplicationUserId)
                .Sum(record => record.Count);

            HttpContext.Session.SetInt32(SD.SessionCart, itemCount);

            return RedirectToAction(nameof(Index));
        }

        private string GetCurrentUserId()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            Debug.Assert(claimsIdentity != null, "User is required to be logged in.");
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            Debug.Assert(claim != null, "All valid users are supposed to have an Id.");
            return claim.Value;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}