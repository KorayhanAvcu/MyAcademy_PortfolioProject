using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;

namespace Portfolio.ViewComponents.Default_Experience
{
    public class _DefaultExperienceViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public _DefaultExperienceViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var experiences = _context.Experiences.ToList();

            // Veri yoksa bolum hic cizilmez.
            if (experiences.Count == 0)
                return Content(string.Empty);

            return View(experiences);
        }
    }
}
