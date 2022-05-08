using BulkyBook.DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        readonly IUnitOfWork _db;

        public OrderController(IUnitOfWork db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _db.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList() });
        }
    }
}
