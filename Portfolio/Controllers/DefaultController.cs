using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using Portfolio.Models;

namespace Portfolio.Controllers
{
    [AllowAnonymous]
    public class DefaultController : Controller
    {
        /// <summary>Formun durumunu bir sonraki istege tasiyan TempData anahtarlari.</summary>
        public const string ContactErrorKey = "IletisimHata";
        public const string ContactSuccessKey = "IletisimBasarili";

        private readonly AppDbContext _context;

        public DefaultController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Ozgecmis bolumunun basligi, iki alt bolumun de verisi yoksa hic cizilmesin.
            ViewData["HasResume"] = _context.Experiences.Any() || _context.Educations.Any();
            return View();
        }

        [HttpPost]
        [EnableRateLimiting("contact")]
        public IActionResult CreateUserMessage(ContactFormVM form)
        {
            // Honeypot dolu: hicbir sey kaydedilmez, ama gonderen basarili sanir.
            if (!string.IsNullOrWhiteSpace(form.Website))
            {
                TempData[ContactSuccessKey] = "Mesajınız alındı. En kısa sürede dönüş yapacağım.";
                return RedirectToAction(nameof(Index));
            }

            // Anasayfa layout'suz tek parca bir view; View("Index") dondurmek sayfayi
            // bozuyordu. Hata mesaji TempData ile tasinip yonlendirme yapiliyor.
            if (!ModelState.IsValid)
            {
                TempData[ContactErrorKey] =
                    "Mesaj gönderilemedi. Lütfen ad, geçerli bir e-posta ve 10-100 karakterlik bir mesaj girin.";
                return RedirectToAction(nameof(Index));
            }

            _context.UserMessages.Add(new UserMessage
            {
                Name = form.Name,
                Email = form.Email,
                MessageBody = form.MessageBody,
            });
            _context.SaveChanges();

            TempData[ContactSuccessKey] = "Mesajınız alındı. En kısa sürede dönüş yapacağım.";
            return RedirectToAction(nameof(Index));
        }
    }
}
