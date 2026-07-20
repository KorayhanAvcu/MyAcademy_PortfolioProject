using Microsoft.AspNetCore.Mvc;

namespace Portfolio.ViewComponents.AdminLayout
{
    public class _AdminLayoutSidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
