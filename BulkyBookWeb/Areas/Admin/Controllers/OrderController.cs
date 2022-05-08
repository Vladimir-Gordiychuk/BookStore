using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using BulkyBook.Utility;
using System.Linq.Expressions;

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
        public IActionResult GetAll(string status)
        {
            Expression<Func<OrderHeader, bool>> filter = (order) => true;

            switch (status)
            {
                case "inprocess":
                    filter = (order) => order.OrderStatus == SD.StatusInProcess;
                    break;
                case "pending":
                    filter = (order) => order.PaymentStatus == SD.PaymentStatusDelayedPayment;
                    break;
                case "completed":
                    filter = (order) => order.OrderStatus == SD.StatusShipped;
                    break;
                case "approved":
                    filter = (order) => order.OrderStatus == SD.StatusApproved;
                    break;
            }

            return Json(new {
                data = _db.OrderHeader
                    .Where(filter, includeProperties: "ApplicationUser")
                    .ToList()
            });
        }
    }
}
