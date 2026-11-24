using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;

namespace Portfolio.ViewComponents.Default_Index
{
    public class _DefaultAboutViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public _DefaultAboutViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var about = _context.Abouts.ToList();

            // Bolum yetkinlikler alt bolumunu de iceriyor: ikisi de bossa hic
            // cizilmez, biri varsa bolum kalir (digeri kendini gizler).
            if (about.Count == 0 && !_context.Skills.Any())
                return Content(string.Empty);

            return View(about);
        }
    }
}
