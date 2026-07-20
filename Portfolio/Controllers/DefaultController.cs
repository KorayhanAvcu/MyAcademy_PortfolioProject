using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;

namespace Portfolio.Controllers
{
    public class DefaultController : Controller
    {
        private readonly AppDbContext _context;

        public DefaultController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateUserMessage(UserMessage userMessage)
        {
            if (!ModelState.IsValid)
            {
                 return View("Index");
            }
            _context.UserMessages.Add(userMessage);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
