using Microsoft.AspNetCore.Mvc;
using Portfolio.Models;

namespace Portfolio.ViewComponents.Default_Head
{
    public class _DefaultHeadViewComponent : ViewComponent
    {
        // Yapilandirma bos birakildiginda kullanilan notr degerler. Gercek metinler
        // icerik kapisinda (Task 13) kullanicidan gelecek.
        private const string DefaultTitle = "Portfolyo";
        private const string DefaultDescription = "Kişisel portfolyo sitesi.";

        private readonly IConfiguration _configuration;

        public _DefaultHeadViewComponent(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IViewComponentResult Invoke()
        {
            var baseUrl = (_configuration["Site:BaseUrl"] ?? string.Empty).Trim().TrimEnd('/');

            var meta = new PageMetaVM
            {
                Title = Value("Site:Title", DefaultTitle),
                Description = Value("Site:Description", DefaultDescription),
                CanonicalUrl = Absolute(baseUrl, CurrentPath()),
                ImageUrl = Absolute(baseUrl, _configuration["Site:OgImage"]),
            };

            return View(meta);
        }

        private string Value(string key, string fallback)
        {
            var value = _configuration[key];
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private string CurrentPath()
        {
            var path = HttpContext?.Request.Path.Value;
            return string.IsNullOrEmpty(path) ? "/" : path;
        }

        /// <summary>
        /// Goreli yolu <paramref name="baseUrl"/> ile mutlaklastirir. Deger zaten
        /// mutlaksa oldugu gibi doner; BaseUrl bos ve deger goreliyse null doner.
        /// </summary>
        private static string? Absolute(string baseUrl, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return value;

            if (baseUrl.Length == 0)
                return null;

            return value.StartsWith('/') ? baseUrl + value : $"{baseUrl}/{value}";
        }
    }
}
