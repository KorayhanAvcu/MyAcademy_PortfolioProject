using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;

namespace Portfolio.ViewComponents.Default_Education
{
    public class _DefaultEducationViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public _DefaultEducationViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var educations = _context.Educations.ToList();
            return View(educations);
        }
    }
}
