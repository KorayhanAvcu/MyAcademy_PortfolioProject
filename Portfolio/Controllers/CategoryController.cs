using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using System.Reflection;

namespace Portfolio.Controllers
{
    public class CategoryController : AdminControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            
            _context.Categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult UpdateCategory(int id)
        {
            var category = _context.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        public IActionResult UpdateCategory(Category category)
        {
            
            _context.Categories.Update(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            _context.Categories.Remove(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
