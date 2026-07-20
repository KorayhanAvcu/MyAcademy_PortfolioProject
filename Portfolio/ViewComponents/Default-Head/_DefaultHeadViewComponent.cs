using Microsoft.AspNetCore.Mvc;

namespace Portfolio.ViewComponents.Default_Head
{
    public class _DefaultHeadViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
