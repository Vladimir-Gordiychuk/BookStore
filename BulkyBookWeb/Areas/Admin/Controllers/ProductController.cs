using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        const int CreateProductId = 0;

        readonly IUnitOfWork _db;
        readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Product product;
            if (id == CreateProductId)
            {
                product = new Product();
            }
            else {
                product = _db.Product.Find(id);
                if (product == null)
                {
                    return NotFound();
                }
            }

            var viewModel = new ProductViewModel
            {
                Product = product,
                Categories = _db.Category.GetAll().ToList(),
                CoverTypes = _db.CoverType.GetAll().ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductViewModel viewModel, IFormFile? file)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"images\products");
                var extension = Path.GetExtension(file.FileName);
                using (var fileStream = new FileStream(
                    Path.Combine(uploads, fileName + extension),
                    FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                viewModel.Product.ImageUrl = @"\images\products\" + fileName + extension;
            }

            if (viewModel.Product.Id == CreateProductId)
            {
                // create new product
                _db.Product.Add(viewModel.Product);
                _db.Save();

                TempData["success"] = $"Product '{viewModel.Product.Title}' updated successfully.";
            }
            else
            {
                // update existing product
                var target = _db.Product.Find(viewModel.Product.Id);
                if (target == null)
                {
                    return NotFound();
                }

                _db.Product.Update(viewModel.Product);
                _db.Save();

                TempData["success"] = $"Product '{viewModel.Product.Title}' updated successfully.";
            }

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
            var target = _db.Product.Find(id);
            if (target == null)
            {
                return NotFound();
            }

            _db.Product.Remove(target);
            _db.Save();

            TempData["success"] = $"Product '{target.Title}' removed successfully.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _db.Product.GetAll(includeProperties: "Category,CoverType").ToList() });
        }

    }
}
