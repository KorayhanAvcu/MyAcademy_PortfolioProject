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
            return View(service);
        }
    }
}
