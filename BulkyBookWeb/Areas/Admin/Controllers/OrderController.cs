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
using Stripe.Checkout;

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

            TempData[SD.TempDataSuccess] = "Order Details updated Successfully";

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

            TempData[SD.TempDataSuccess] = "Order Status updated Successfully";

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

            TempData[SD.TempDataSuccess] = "Order Status updated Successfully";

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

            TempData[SD.TempDataSuccess] = "Order Cancelled Successfully";

            return RedirectToAction(nameof(Details), new { id = header.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SD.RoleCustomerCompany)]
        public IActionResult PayNow(OrderVm orderVm)
        {
            var orderHeader = _db.OrderHeader.Find(orderVm.Header.Id);

            if (orderHeader == null)
            {
                return NotFound();
            }

            var items = _db.OrderDetail.Where(item => item.OrderId == orderHeader.Id, nameof(OrderDetail.Product));

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
                SuccessUrl = domain + $"Admin/Order/{nameof(PaymentConfirmation)}?id={orderHeader.Id}",
                CancelUrl = domain + $"Admin/Order/{nameof(Details)}?id={orderHeader.Id}",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            orderHeader.SessionId = session.Id;
            orderHeader.PaymentIntentId = session.PaymentIntentId;

            _db.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int id)
        {
            var order = _db.OrderHeader.Find(id);

            Debug.Assert(order.PaymentStatus == SD.PaymentStatusDelayedPayment);

            var service = new SessionService();
            Session session = service.Get(order.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                order.PaymentStatus = SD.PaymentStatusApproved;
                order.PaymentDate = DateTime.Now;
                _db.Save();
            }

            TempData[SD.TempDataSuccess] = "Payment Accepted";

            return View(id);
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
