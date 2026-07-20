using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;

namespace Portfolio.ViewComponents.Default_Footer
{
    public class _DefaultFooterViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public _DefaultFooterViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var contantinfo = _context.ContactInfos.ToList();
            return View(contantinfo);
        }
    }
}
