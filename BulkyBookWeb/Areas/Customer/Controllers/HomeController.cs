using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

        public IActionResult Details(int id)
        {
            var cart = new ShoppingCart()
            {
                Product = _db.Product.GetFirstOrDefault(item => item.Id == id, includeProperties: CategoryAndCoverType),
                Count = 1
            };

            return View(cart);
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