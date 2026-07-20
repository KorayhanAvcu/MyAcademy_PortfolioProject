using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;

namespace Portfolio.Controllers
{
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var services = _context.Services.ToList();
            return View(services);
        }

        public IActionResult CreateService()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateService(Service service)
        {
            if (!ModelState.IsValid)
            {
                return View(service);
            }
            _context.Services.Add(service);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult UpdateService(int id)
        {
            var service = _context.Services.Find(id);
            return View(service);
        }
        [HttpPost]
        public IActionResult UpdateService(Service service)
        {
            if (!ModelState.IsValid)
            {
                return View(service);
            }
            _context.Services.Update(service);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteService(int id)
        {
            var service = _context.Services.Find(id);
            _context.Services.Remove(service);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
