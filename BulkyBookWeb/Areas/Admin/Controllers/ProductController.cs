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

            
            if (file != null)
            {
                // todo: For security reasons it would be better
                // to request existing ImageUrl from database. 

                FileInfo fileInfo = GetImagePath(viewModel, file);
                using (var fileStream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.Write))
                {
                    file.CopyTo(fileStream);
                }
                viewModel.Product.ImageUrl = @"\images\products\" + fileInfo.Name;
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
                _db.Product.Update(viewModel.Product);
                _db.Save();

                TempData["success"] = $"Product '{viewModel.Product.Title}' updated successfully.";
            }

            return RedirectToAction("Index");
        }

        private FileInfo GetImagePath(ProductViewModel viewModel, IFormFile? file)
        {
            FileInfo fileInfo;
            if (viewModel.Product.ImageUrl != null)
            {
                // Product already exists in a database and has an image.
                fileInfo = GetImagePath(viewModel.Product.ImageUrl);
            }
            else
            {
                fileInfo = GenerateImagePath(Path.GetExtension(file.FileName));
            }

            return fileInfo;
        }

        private FileInfo GetImagePath(string imageUrl)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;

            var fileName = Path.GetFileNameWithoutExtension(imageUrl);
            var extension = Path.GetExtension(imageUrl);
            var uploads = Path.Combine(wwwRootPath, @"images\products");

            var filePath = Path.Combine(uploads, fileName + extension);
            return new FileInfo(filePath);
        }

        private FileInfo GenerateImagePath(string extension)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;

            var fileName = Guid.NewGuid().ToString();
            var uploads = Path.Combine(wwwRootPath, @"images\products");
            var filePath = Path.Combine(uploads, fileName + extension);
            return new FileInfo(filePath);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _db.Product.GetAll(includeProperties: "Category,CoverType").ToList() });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var target = _db.Product.Find(id);
            if (target == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Specified Product not found."
                });
            }

            var imageFile = GetImagePath(target.ImageUrl);
            imageFile.Delete();

            _db.Product.Remove(target);
            _db.Save();

            return Json(new
            {
                success = true,
                message = "Product deleted successfully."
            });
        }

    }
}
