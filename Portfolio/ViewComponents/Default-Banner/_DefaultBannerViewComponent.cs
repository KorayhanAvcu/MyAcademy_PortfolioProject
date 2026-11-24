using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;

namespace Portfolio.ViewComponents.Default_Banner
{
    public class _DefaultBannerViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public _DefaultBannerViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var banner = _context.Banners.ToList();

            // Veri yoksa bolum hic cizilmez.
            if (banner.Count == 0)
                return Content(string.Empty);

            return View(banner);
        }
    }
}
