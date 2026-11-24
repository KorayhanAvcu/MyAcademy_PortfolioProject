using Microsoft.AspNetCore.Mvc;

namespace Portfolio.Controllers
{
    public class AdminLayoutController : AdminControllerBase
    {
        public IActionResult Layout()
        {
            return View();
        }
    }
}
