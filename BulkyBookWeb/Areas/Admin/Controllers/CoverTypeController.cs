using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class CoverTypeController : Controller
    {
        readonly IUnitOfWork _db;

        public CoverTypeController(IUnitOfWork db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View(_db.CoverType.GetAll().ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType coverType)
        {
            if (!ModelState.IsValid)
            {
                return View(coverType);
            }

            _db.CoverType.Add(coverType);
            _db.Save();

            TempData["success"] = $"Cover Type '{coverType.Name}' created successfully.";

            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var coverType = _db.CoverType.Find(id);
            if (coverType == null)
            {
                return NotFound();
            }
            return View(coverType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType coverType)
        {
            if (!ModelState.IsValid)
                return View(coverType);

            var target = _db.CoverType.Find(coverType.Id);
            if (target == null)
            {
                return NotFound();
            }

            target.Name = coverType.Name;

            _db.CoverType.Update(target);
            _db.Save();

            TempData["success"] = $"Cover Type '{target.Name}' updated successfully.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var coverType = _db.CoverType.Find(id);
            if (coverType == null)
            {
                return NotFound();
            }
            return View(coverType);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int id)
        {
            var target = _db.CoverType.Find(id);
            if (target == null)
            {
                return NotFound();
            }

            _db.CoverType.Remove(target);
            _db.Save();

            TempData["success"] = $"Cover Type '{target.Name}' removed successfully.";

            return RedirectToAction("Index");
        }
    }
}
