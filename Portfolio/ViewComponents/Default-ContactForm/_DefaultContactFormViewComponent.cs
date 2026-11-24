using Microsoft.AspNetCore.Mvc;
using Portfolio.Models;

namespace Portfolio.ViewComponents.Default_ContactForm
{
    public class _DefaultContactFormViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            // Form entity'ye degil, kendi view model'ine bagli (honeypot dahil).
            return View(new ContactFormVM());
        }
    }
}
