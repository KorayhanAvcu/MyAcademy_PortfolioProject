using Microsoft.AspNetCore.Mvc;

namespace Portfolio.ViewComponents.AdminLayout
{
    public class _AdminLayoutSidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            ViewBag.fullName = UserClaimsPrincipal.FindFirst("fullname")?.Value;
            ViewBag.imgUrl = UserClaimsPrincipal.FindFirst("imageUrl")?.Value;
            return View();
        }
    }
}
