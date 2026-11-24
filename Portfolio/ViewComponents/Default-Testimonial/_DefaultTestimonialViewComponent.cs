using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;

namespace Portfolio.ViewComponents.Default_Testimonial
{
    public class _DefaultTestimonialViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public _DefaultTestimonialViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var testimonials = _context.Testimonials.ToList();

            // Uydurma referanslar silindiginde bolum tumuyle kaybolur.
            if (testimonials.Count == 0)
                return Content(string.Empty);

            return View(testimonials);
        }
    }
}
