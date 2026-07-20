using Microsoft.AspNetCore.Mvc;

namespace Portfolio.ViewComponents.AdminLayout
{
    public class _AdminLayoutHeadViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
