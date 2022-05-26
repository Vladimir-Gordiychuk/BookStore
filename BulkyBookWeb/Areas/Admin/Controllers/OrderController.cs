using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using BulkyBook.Utility;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BulkyBook.Models.ViewModels;
using Stripe;

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

        public IActionResult Details(int id)
        {
            var header = _db.OrderHeader
                .GetFirstOrDefault(order => order.Id == id, includeProperties: "ApplicationUser");

            if (header == null)
            {
                return NotFound();
            }

            var details = _db.OrderDetail
                .Where(detail => detail.OrderId == id, includeProperties: "Product");

            var order = new OrderVm
            {
                Header = header,
                Details = details.ToList()
            };

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult Details(OrderVm orderVm)
        {
            var order = orderVm.Header;
            var header = _db.OrderHeader.Find(order.Id);

            if (header == null)
            {
                return NotFound();
            }

            header.Name = order.Name;
            header.PhoneNumber = order.PhoneNumber;
            header.StreetAddress = order.StreetAddress;
            header.City = order.City;
            header.State = order.State;
            header.PostalCode = order.PostalCode;

            if (order.Carrier != null)
            {
                header.Carrier = order.Carrier;
            }

            if (order.TrackingNumber != null)
            {
                header.TrackingNumber = order.TrackingNumber;
            }

            _db.Save();

            TempData["Success"] = "Order Details updated Successfully";

            return RedirectToAction(nameof(Details), new { id = header.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult StartProcessing(OrderVm orderVm)
        {
            var order = orderVm.Header;
            var header = _db.OrderHeader.Find(order.Id);

            if (header == null)
            {
                return NotFound();
            }

            header.OrderStatus = SD.StatusInProcess;

            _db.Save();

            TempData["Success"] = "Order Status updated Successfully";

            return RedirectToAction(nameof(Details), new { id = header.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult ShipOrder(OrderVm orderVm)
        {
            var order = orderVm.Header;
            var header = _db.OrderHeader.Find(order.Id);

            if (header == null)
            {
                return NotFound();
            }

            header.TrackingNumber = order.TrackingNumber;
            header.Carrier = order.Carrier;
            header.OrderStatus = SD.StatusShipped;
            header.ShippingDate = DateTime.Now;

            if (header.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                header.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            _db.Save();

            TempData["Success"] = "Order Status updated Successfully";

            return RedirectToAction(nameof(Details), new { id = header.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult CancelOrder(OrderVm orderVm)
        {
            var order = orderVm.Header;
            var header = _db.OrderHeader.Find(order.Id);

            if (header == null)
            {
                return NotFound();
            }

            if (header.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = header.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                header.OrderStatus = SD.StatusCancelled;
                header.PaymentStatus = SD.StatusRefunded;
            }
            else
            {
                header.OrderStatus = SD.StatusCancelled;
                header.PaymentStatus = SD.StatusCancelled;
            }

            _db.Save();

            TempData["Success"] = "Order Cancelled Successfully";

            return RedirectToAction(nameof(Details), new { id = header.Id });
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
