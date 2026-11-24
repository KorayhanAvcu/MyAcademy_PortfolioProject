using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;

namespace Portfolio.Controllers
{
    public class ExperienceController : AdminControllerBase
    {
        private readonly AppDbContext _context;

        public ExperienceController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var experiences= _context.Experiences.ToList();
            return View(experiences);
        }

        [HttpGet]
        public IActionResult CreateExperience()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateExperience(Experience experience)
        {
            if (!ModelState.IsValid)
            {
                return View(experience);
            }
            _context.Experiences.Add(experience);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult UpdateExperience(int id)
        {
            var experience = _context.Experiences.Find(id);
            return View(experience);
        }

        [HttpPost]
        public IActionResult UpdateExperience(Experience experience)
        {
            if (!ModelState.IsValid)
            {
                return View(experience);
            }
            _context.Experiences.Update(experience);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteExperience(int id)
        {
            var experience = _context.Experiences.Find(id);
            _context.Experiences.Remove(experience);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
