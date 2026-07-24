using Microsoft.AspNetCore.Mvc;

namespace Portfolio.ViewComponents.AdminLayout
{
    public class _AdminLayoutSidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            ViewBag.fullName = HttpContext.Session.GetString("fullName");
            ViewBag.imgUrl = HttpContext.Session.GetString("imageUrl");
            return View();
        }
    }
}
