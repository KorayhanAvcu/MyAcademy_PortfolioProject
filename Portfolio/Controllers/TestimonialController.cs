using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;

namespace Portfolio.Controllers
{
    public class TestimonialController : Controller
    {
        private readonly AppDbContext _context;

        public TestimonialController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var testimonials = _context.Testimonials.ToList();
            return View(testimonials);
        }
        
        public IActionResult CreateTestimonial()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateTestimonial(Testimonial testimonial)
        {
            if (!ModelState.IsValid)
            {
                return View(testimonial);
            }
            _context.Testimonials.Add(testimonial);
            _context.SaveChanges();
            return RedirectToAction("Index");   
        }

        public IActionResult UpdateTestimonial(int id)
        {
            var testimonial = _context.Testimonials.Find(id);
            return View(testimonial);
        }

        [HttpPost]
        public IActionResult UpdateTestimonial(Testimonial testimonial)
        {
            if (!ModelState.IsValid)
            {
                return View(testimonial);
            }
            _context.Testimonials.Update(testimonial);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteTestimonial(int id)
        {
            var testimonial = _context.Testimonials.Find(id);
            _context.Testimonials.Remove(testimonial);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
