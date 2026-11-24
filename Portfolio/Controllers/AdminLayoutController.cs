using Microsoft.AspNetCore.Mvc;

namespace Portfolio.Controllers
{
    public class AdminLayoutController : AdminControllerBase
    {
        public IActionResult Layout()
        {
            return View();
        }

        public PartialViewResult HeadPartial()
        {
            return PartialView();
        }
    }
}
