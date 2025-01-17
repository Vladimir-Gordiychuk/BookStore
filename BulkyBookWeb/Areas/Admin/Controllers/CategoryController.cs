﻿using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class CategoryController : Controller
    {
        readonly IUnitOfWork _db;

        public CategoryController(IUnitOfWork db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var categories = _db.Category.GetAll().ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError(
                    "NameEqualsDisplayOrder",
                    $"The {nameof(Category.DisplayOrder)} cannot exactly match {nameof(Category.Name)}.");
            }

            if (!ModelState.IsValid)
                return View(category);

            _db.Category.Add(category);

            _db.Save();

            TempData[SD.TempDataSuccess] = $"New category '{category.Name}' created.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _db.Category.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError(
                    "NameEqualsDisplayOrder",
                    $"The {nameof(Category.DisplayOrder)} cannot exactly match {nameof(Category.Name)}.");
            }

            if (!ModelState.IsValid)
                return View(category);

            var target = _db.Category.Find(category.Id);
            if (target == null)
            {
                return NotFound();
            }

            target.Name = category.Name;
            target.DisplayOrder = category.DisplayOrder;

            _db.Save();

            TempData[SD.TempDataSuccess] = $"Category '{category.Name}' updated.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = _db.Category.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int id)
        {
            var target = _db.Category.Find(id);
            if (target != null)
            {
                _db.Category.Remove(target);
                _db.Save();

                TempData[SD.TempDataSuccess] = $"Category '{target.Name}' deleted.";
            }

            return RedirectToAction("Index");
        }


    }
}
