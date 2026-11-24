using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using Portfolio.Services;

namespace Portfolio.Controllers
{
    public class BannerController : AdminControllerBase
    {
        private const string UploadFolder = "banner";

        private readonly AppDbContext _context;
        private readonly IImageStorage _imageStorage;

        public BannerController(AppDbContext context, IImageStorage imageStorage)
        {
            _context = context;
            _imageStorage = imageStorage;
        }

        public IActionResult Index()
        {
            var banner = _context.Banners.FirstOrDefault();
            return View(banner);
        }

        [HttpGet]
        public IActionResult CreateBanner()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBanner(Banner banner, CancellationToken ct)
        {
            // ImageUrl'in kendi zorunlulugu kalkti: dosya ya da URL yeterli.
            ModelState.Remove(nameof(Banner.ImageUrl));

            if (!ModelState.IsValid)
            {
                return View(banner);
            }

            if (banner.ImageFile != null)
            {
                var result = await _imageStorage.SaveAsync(banner.ImageFile, UploadFolder, ct);
                if (!result.Success)
                {
                    ModelState.AddModelError(nameof(Banner.ImageFile), result.ErrorMessage!);
                    return View(banner);
                }

                banner.ImageUrl = result.RelativeUrl!;
            }

            if (string.IsNullOrWhiteSpace(banner.ImageUrl))
            {
                ModelState.AddModelError(nameof(Banner.ImageUrl), ImageRequiredMessage);
                return View(banner);
            }

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync(ct);
            return RedirectToAction("Index");
        }

        public IActionResult UpdateBanner(int id)
        {
            var banner = _context.Banners.Find(id);
            return View(banner);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBanner(Banner banner, CancellationToken ct)
        {
            ModelState.Remove(nameof(Banner.ImageUrl));

            if (!ModelState.IsValid)
            {
                return View(banner);
            }

            var current = await _context.Banners
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == banner.Id, ct);

            if (current == null)
            {
                return NotFound();
            }

            // Yeni dosya basariyla kaydedilirse eski yuklenmis dosya kayittan SONRA silinir.
            string? replacedUpload = null;

            if (banner.ImageFile != null)
            {
                var result = await _imageStorage.SaveAsync(banner.ImageFile, UploadFolder, ct);
                if (!result.Success)
                {
                    ModelState.AddModelError(nameof(Banner.ImageFile), result.ErrorMessage!);
                    return View(banner);
                }

                replacedUpload = current.ImageUrl;
                banner.ImageUrl = result.RelativeUrl!;
            }
            else if (string.IsNullOrWhiteSpace(banner.ImageUrl))
            {
                // Dosya secilmedi ve URL bos geldi: mevcut gorsel aynen korunur.
                banner.ImageUrl = current.ImageUrl;
            }

            if (string.IsNullOrWhiteSpace(banner.ImageUrl))
            {
                ModelState.AddModelError(nameof(Banner.ImageUrl), ImageRequiredMessage);
                return View(banner);
            }

            _context.Banners.Update(banner);
            await _context.SaveChangesAsync(ct);

            _imageStorage.Delete(replacedUpload);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteBanner(int id)
        {
            var banner = _context.Banners.Find(id);
            if (banner == null)
            {
                return NotFound();
            }

            var imageUrl = banner.ImageUrl;

            _context.Banners.Remove(banner);
            _context.SaveChanges();

            _imageStorage.Delete(imageUrl);
            return RedirectToAction("Index");
        }
    }
}
