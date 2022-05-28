using BulkyBook.DataAccess.Repository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        readonly IUnitOfWork _db;

        public ShoppingCartViewComponent(IUnitOfWork db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim is null)
            {
                HttpContext.Session.Clear();
                return View(0);
            }

            var sessionValue = HttpContext.Session.GetInt32(SD.SessionCart);
            int itemCount = sessionValue ?? UpdateSessionCart(claim.Value);

            return View(itemCount);
        }

        private int UpdateSessionCart(string userId)
        {
            var itemCount = _db.ShoppingCart
                .Where(record => record.ApplicationUserId == userId)
                .Sum(record => record.Count);

            HttpContext.Session.SetInt32(SD.SessionCart, itemCount);

            return itemCount;
        }

    }
}
