using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;

namespace Portfolio.Controllers
{
    public class AboutController : AdminControllerBase
    {
        private readonly AppDbContext _context;

        public AboutController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var about = _context.Abouts.FirstOrDefault();
            return View(about);
        }

        [HttpGet]
        public IActionResult CreateAbout()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateAbout(About about)
        {
            if (!ModelState.IsValid)
            {
                return View(about);
            }
            _context.Abouts.Add(about);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult UpdateAbout(int id)
        {
            var about = _context.Abouts.Find(id);
            return View(about);
        }

        [HttpPost]
        public IActionResult UpdateAbout(About about)
        {
            if (!ModelState.IsValid)
            {
                return View(about);
            }
            _context.Abouts.Update(about);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteAbout(int id)
        {
            var about = _context.Abouts.Find(id);
            _context.Abouts.Remove(about);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
