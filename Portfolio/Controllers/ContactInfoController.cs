using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;

namespace Portfolio.Controllers
{
    public class ContactInfoController : Controller
    {
        private readonly AppDbContext _context;

        public ContactInfoController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var contactInfo = _context.ContactInfos.FirstOrDefault();
            return View(contactInfo);
        }
        [HttpGet]
        public IActionResult CreateContactInfo()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateContactInfo(ContactInfo contactInfo)
        {
            if (!ModelState.IsValid)
            {
                return View(contactInfo);
            }
            _context.ContactInfos.Add(contactInfo);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult UpdateContactInfo(int id)
        {
            var contactInfo = _context.ContactInfos.Find(id);
            return View(contactInfo);
        }

        [HttpPost]
        public IActionResult UpdateContactInfo(ContactInfo contactInfo)
        {
            if (!ModelState.IsValid)
            {
                return View(contactInfo);
            }
            _context.ContactInfos.Update(contactInfo);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteContactInfo(int id)
        {
            var contactInfo = _context.ContactInfos.Find(id);
            _context.ContactInfos.Remove(contactInfo);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
