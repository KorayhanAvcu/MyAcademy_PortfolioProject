using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;

namespace Portfolio.Controllers
{
    public class UserMessageController : Controller
    {
        private readonly AppDbContext _context;

        public UserMessageController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(bool? isRead)
        {
            ViewBag.IsRead = isRead;

            var messages = _context.UserMessages.AsQueryable();

            if (isRead.HasValue)
                messages = messages.Where(x => x.IsRead == isRead.Value);

            return View(messages.ToList());
        }

        public IActionResult UpdateUserMessage(int id)
        {
            var usermessage = _context.UserMessages.Find(id);
            return View(usermessage);
        }

        [HttpPost]
        public IActionResult UpdateUserMessage(UserMessage userMessage)
        {
            _context.UserMessages.Update(userMessage);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteUserMessage(int id)
        {
            var message = _context.UserMessages.Find(id);
            _context.UserMessages.Remove(message);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
