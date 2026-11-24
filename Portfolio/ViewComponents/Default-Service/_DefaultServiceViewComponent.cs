using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;

namespace Portfolio.ViewComponents.Default_Service
{
    public class _DefaultServiceViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public _DefaultServiceViewComponent(AppDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke()
        {
            var service = _context.Services.ToList();

            // Veri yoksa bolum hic cizilmez.
            if (service.Count == 0)
                return Content(string.Empty);

            return View(service);
        }
    }
}
