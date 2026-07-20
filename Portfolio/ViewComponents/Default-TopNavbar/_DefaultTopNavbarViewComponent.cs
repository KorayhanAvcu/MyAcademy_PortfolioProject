using Microsoft.AspNetCore.Mvc;

namespace Portfolio.ViewComponents.Default_TopNavbar
{
    public class _DefaultTopNavbarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
