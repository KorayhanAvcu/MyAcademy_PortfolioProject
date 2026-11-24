using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using Portfolio.Services;

namespace Portfolio.Controllers
{
    public class AboutController : AdminControllerBase
    {
        private const string UploadFolder = "about";

        private readonly AppDbContext _context;
        private readonly IImageStorage _imageStorage;

        public AboutController(AppDbContext context, IImageStorage imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        public IActionResult Index()
        {
            var about = _context.Abouts.FirstOrDefault();
            return View(about);
        }

        [HttpGet]
        public IActionResult CreateAbout()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAbout(About about, CancellationToken ct)
        {
            // ImageUrl'in kendi zorunlulugu kalkti: dosya ya da URL yeterli.
            ModelState.Remove(nameof(About.ImageUrl));

            if (!ModelState.IsValid)
            {
                return View(about);
            }

            if (about.ImageFile != null)
            {
                var result = await _imageStorage.SaveAsync(about.ImageFile, UploadFolder, ct);
                if (!result.Success)
                {
                    ModelState.AddModelError(nameof(About.ImageFile), result.ErrorMessage!);
                    return View(about);
                }

                about.ImageUrl = result.RelativeUrl!;
            }

            if (string.IsNullOrWhiteSpace(about.ImageUrl))
            {
                ModelState.AddModelError(nameof(About.ImageUrl), ImageRequiredMessage);
                return View(about);
            }

            _context.Abouts.Add(about);
            await _context.SaveChangesAsync(ct);
            return RedirectToAction("Index");
        }

        public IActionResult UpdateAbout(int id)
        {
            var about = _context.Abouts.Find(id);
            return View(about);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAbout(About about, CancellationToken ct)
        {
            ModelState.Remove(nameof(About.ImageUrl));

            if (!ModelState.IsValid)
            {
                return View(about);
            }

            var current = await _context.Abouts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == about.Id, ct);

            if (current == null)
            {
                return NotFound();
            }

            // Yeni dosya basariyla kaydedilirse eski yuklenmis dosya kayittan SONRA silinir.
            string? replacedUpload = null;

            if (about.ImageFile != null)
            {
                var result = await _imageStorage.SaveAsync(about.ImageFile, UploadFolder, ct);
                if (!result.Success)
                {
                    ModelState.AddModelError(nameof(About.ImageFile), result.ErrorMessage!);
                    return View(about);
                }

                replacedUpload = current.ImageUrl;
                about.ImageUrl = result.RelativeUrl!;
            }
            else if (string.IsNullOrWhiteSpace(about.ImageUrl))
            {
                // Dosya secilmedi ve URL bos geldi: mevcut gorsel aynen korunur.
                about.ImageUrl = current.ImageUrl;
            }

            if (string.IsNullOrWhiteSpace(about.ImageUrl))
            {
                ModelState.AddModelError(nameof(About.ImageUrl), ImageRequiredMessage);
                return View(about);
            }

            _context.Abouts.Update(about);
            await _context.SaveChangesAsync(ct);

            _imageStorage.Delete(replacedUpload);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteAbout(int id)
        {
            var about = _context.Abouts.Find(id);
            if (about == null)
            {
                return NotFound();
            }

            var imageUrl = about.ImageUrl;

            _context.Abouts.Remove(about);
            _context.SaveChanges();

            _imageStorage.Delete(imageUrl);
            return RedirectToAction("Index");
        }
    }
}
