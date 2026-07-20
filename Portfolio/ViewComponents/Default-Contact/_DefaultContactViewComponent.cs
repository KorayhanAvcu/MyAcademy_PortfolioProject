using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;

namespace Portfolio.ViewComponents.Default_Contact
{
    public class _DefaultContactViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        public _DefaultContactViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var contact = _context.ContactInfos.ToList();
            return View(contact);
        }
    }
}
