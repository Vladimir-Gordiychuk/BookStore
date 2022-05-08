using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using BulkyBook.Utility;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
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
            Expression<Func<OrderHeader, bool>> filterExpression = GetStatusFilterExpression(status);

            IEnumerable<OrderHeader> orders;

            if (User.IsInRole(SD.RoleAdmin) || User.IsInRole(SD.RoleEmployee))
            {
                orders = _db.OrderHeader
                    .Where(filterExpression, includeProperties: "ApplicationUser");
            }
            else
            {
                var userId = GetCurrentUserId();

                var filterFunction = filterExpression.Compile();

                orders = _db.OrderHeader
                    .Where(order => order.ApplicationUserId == userId, includeProperties: "ApplicationUser")
                    .Where(filterFunction);
            }

            return Json(new
            {
                data = orders
            });
        }

        private static Expression<Func<OrderHeader, bool>> GetStatusFilterExpression(string status)
        {
            Expression<Func<OrderHeader, bool>> filterExpression = (order) => true;

            switch (status)
            {
                case "inprocess":
                    filterExpression = (order) => order.OrderStatus == SD.StatusInProcess;
                    break;
                case "pending":
                    filterExpression = (order) => order.PaymentStatus == SD.PaymentStatusDelayedPayment;
                    break;
                case "completed":
                    filterExpression = (order) => order.OrderStatus == SD.StatusShipped;
                    break;
                case "approved":
                    filterExpression = (order) => order.OrderStatus == SD.StatusApproved;
                    break;
            }

            return filterExpression;
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
