using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;

namespace Portfolio.Controllers
{
    public class BannerController : AdminControllerBase
    {
        private readonly AppDbContext _context;

        public BannerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var banner = _context.Banners.FirstOrDefault();
            return View(banner);
        }
        [HttpGet]
        public IActionResult CreateBanner()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateBanner(Banner banner)
        {
            if (!ModelState.IsValid)
            {
                return View(banner);
            }
            _context.Banners.Add(banner);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult UpdateBanner(int id)
        {
            var banner = _context.Banners.Find(id);
            return View(banner);
        }

        [HttpPost]
        public IActionResult UpdateBanner(Banner banner)
        {
            if (!ModelState.IsValid)
            {
                return View(banner);
            }
            _context.Banners.Update(banner);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteBanner(int id)
        {
            var banner = _context.Banners.Find(id);
            _context.Banners.Remove(banner);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
