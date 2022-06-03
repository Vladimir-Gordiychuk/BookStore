using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using BulkyBook.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class ProductController : Controller
    {
        public const string ImageSizeLimit = "ImageSizeLimit";

        const int CreateProductId = 0;

        readonly IUnitOfWork _db;
        int _imageSizeLimit;

        public ProductController(IUnitOfWork db, IOptions<ApplicationConfig> config)
        {
            _db = db;
            _imageSizeLimit = config.Value.ImageSizeLimit;
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
            {
                viewModel.Categories = _db.Category.GetAll().ToList();
                viewModel.CoverTypes = _db.CoverType.GetAll().ToList();
                return View(viewModel);
            }

            var product = viewModel.Product;

            if (product.Id == CreateProductId)
            {
                // create new product
                _db.Product.Add(product);
                _db.Save();

                TempData[SD.TempDataSuccess] = $"Product '{product.Title}' created successfully.";
            }
            else
            {
                // update existing product
                var target = _db.Product.Find(product.Id);
                if (target is null)
                {
                    return NotFound();
                }

                target.Title = product.Title;
                target.ISBN = product.ISBN;
                target.ListPrice = product.ListPrice;
                target.Price = product.Price;
                target.Price50 = product.Price50;
                target.Price100 = product.Price100;
                target.CategoryId = product.CategoryId;
                target.CoverTypeId = product.CoverTypeId;

                _db.Save();

                TempData[SD.TempDataSuccess] = $"Product '{viewModel.Product.Title}' updated successfully.";
            }

            if (file != null)
            {
                if (file.Length > _imageSizeLimit)
                {
                    TempData[SD.TempDataError] = "Image size is too big.";
                    viewModel.Categories = _db.Category.GetAll().ToList();
                    viewModel.CoverTypes = _db.CoverType.GetAll().ToList();
                    return View(viewModel);
                }

                UpdateProductImage(viewModel.Product.Id, file);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Upload image for specified product.
        /// </summary>
        /// <param name="productId">Existing product id.</param>
        /// <param name="file">New image. If null - method returns immediately.</param>
        private void UpdateProductImage(int productId, IFormFile file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (file.Length > _imageSizeLimit)
            {
                throw new ArgumentException("Image size is too big.", nameof(file));
            }

            var product = _db.Product.Find(productId);
            if (product is null)
            {
                throw new ArgumentException("Invalid product id.", nameof(productId));
            }

            var newImage = new Image
            {
                ContentType = file.ContentType,
                Extension = Path.GetExtension(file.FileName)
            };

            using (var stream = new MemoryStream((int)file.Length))
            {
                file.CopyTo(stream);
                newImage.Content = stream.ToArray();
            }

            Image existingImage = null;
            if (product.ImageId != null && product.ImageId != 0)
            {
                existingImage = _db.Image.Find(product.ImageId.Value);
            }

            if (existingImage is null)
            {
                // create new image

                _db.Image.Add(newImage);
                _db.Save(); // this save method is required to get image id.

                product.ImageId = newImage.Id;
                product.ImageUrl = GetImageUrl(newImage);
            }
            else
            {
                // update existing image

                existingImage.Content = newImage.Content;

                // Db.Save() is not called, because it will called later on.
            }

            _db.Save();
        }

        private static string GetImageUrl(Image newImage)
        {
            return "/image/" + newImage.Id;
        }

        [AllowAnonymous]
        [Route("image/{id}")]
        public IActionResult Image(int id)
        {
            var image = _db.Image.Find(id);

            if (image == null)
                return NotFound();

            return File(image.Content, image.ContentType);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _db.Product.GetAll(includeProperties: "Category,CoverType").ToList() });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Product target = _db.Product.Find(id);
            if (target == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Specified Product not found."
                });
            }

            RemoveProductImage(target);

            _db.Product.Remove(target);
            _db.Save();

            return Json(new
            {
                success = true,
                message = "Product deleted successfully."
            });
        }

        private void RemoveProductImage(Product product)
        {
            if (product.ImageId.HasValue)
            {
                var image = _db.Image.Find(product.ImageId.Value);
                if (image != null)
                {
                    _db.Image.Remove(image);
                }
            }
        }
    }
}
