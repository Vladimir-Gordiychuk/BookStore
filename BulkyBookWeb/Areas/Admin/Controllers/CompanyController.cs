using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        const int CreateCompanyId = 0;

        readonly IUnitOfWork _db;

        public CompanyController(IUnitOfWork db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Company company;
            if (id == CreateCompanyId)
            {
                company = new Company();
            }
            else {
                company = _db.Company.Find(id);
                if (company == null)
                {
                    return NotFound();
                }
            }

            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Company company)
        {
            if (!ModelState.IsValid)
                return View(company);

            if (company.Id == CreateCompanyId)
            {
                // create new company
                _db.Company.Add(company);
                _db.Save();

                TempData["success"] = $"Company '{company.Name}' updated successfully.";
            }
            else
            {
                // update existing company
                _db.Company.Update(company);
                _db.Save();

                TempData["success"] = $"Company '{company.Name}' updated successfully.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _db.Company.GetAll().ToList() });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var target = _db.Company.Find(id);
            if (target == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Specified Company not found."
                });
            }

            _db.Company.Remove(target);
            _db.Save();

            return Json(new
            {
                success = true,
                message = "Company deleted successfully."
            });
        }

    }
}
