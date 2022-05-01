using BulkyBook.DataAccess.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        readonly IUnitOfWork _db;

        public CartController(IUnitOfWork db)
        {
            _db = db;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
