using Microsoft.Extensions.Logging;

namespace Portfolio.Services
{
    /// <summary>
    /// Gorselleri <c>wwwroot/uploads/&lt;folder&gt;</c> altina yazar. Yeniden kodlama
    /// yapilmaz; dosya oldugu gibi saklanir, bu yuzden kabul kriterleri sikidir:
    /// uzanti, boyut ve dosyanin bas baytlari birlikte dogrulanir.
    /// </summary>
    public sealed class LocalImageStorage : IImageStorage
    {
        /// <summary>Goreli yollarin ortak oneki; <see cref="Delete"/> bunun disina cikmaz.</summary>
        public const string UploadsPrefix = "/uploads/";

        /// <summary>Uygulama sinirinin ustunde kalan ham istek govdesi Kestrel'de ayarlanir (Program.cs).</summary>
        public const long MaxFileSizeBytes = 2 * 1024 * 1024;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalImageStorage> _logger;

        public LocalImageStorage(IWebHostEnvironment environment, ILogger<LocalImageStorage> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<ImageSaveResult> SaveAsync(IFormFile file, string folder, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                return new ImageSaveResult(false, null, "Dosya boş.");

            // 1) Uzanti: kucuk harfe cevrilip karsilastirilir, boylece ".JPG" de kabul edilir.
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
            if (!AllowedExtensions.Contains(extension))
                return new ImageSaveResult(
                    false, null, "Yalnızca .jpg, .jpeg, .png ve .webp uzantılı görseller yüklenebilir.");

            // 2) Boyut.
            if (file.Length > MaxFileSizeBytes)
                return new ImageSaveResult(false, null, "Görsel en fazla 2 MB olabilir.");

            // 3) Sihirli baytlar: uzantisi degistirilmis bir dosya buradan gecemez.
            var header = new byte[12];
            int read;
            await using (var probe = file.OpenReadStream())
            {
                read = await ReadExactlyAsync(probe, header, ct);
            }

            if (!HasKnownImageSignature(header, read))
                return new ImageSaveResult(
                    false, null, "Dosya içeriği geçerli bir JPEG, PNG veya WEBP görseli değil.");

            // Istemcinin dosya adi hic kullanilmaz: ad da uzanti da bizim belirledigimiz degerler.
            var safeFolder = SanitizeFolder(folder);
            var fileName = Guid.NewGuid().ToString("N") + extension;

            var folderPath = Path.Combine(GetUploadsRoot(), safeFolder);
            Directory.CreateDirectory(folderPath);

            var physicalPath = Path.Combine(folderPath, fileName);
            await using (var target = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await using var source = file.OpenReadStream();
                await source.CopyToAsync(target, ct);
            }

            return new ImageSaveResult(true, $"{UploadsPrefix}{safeFolder}/{fileName}", null);
        }

        public void Delete(string? relativeUrl)
        {
            // Harici URL'ler (live veride bunlar var) ve bos degerler korunur.
            if (string.IsNullOrWhiteSpace(relativeUrl) ||
                !relativeUrl.StartsWith(UploadsPrefix, StringComparison.OrdinalIgnoreCase))
                return;

            var uploadsRoot = Path.GetFullPath(GetUploadsRoot());
            var relativePath = relativeUrl[UploadsPrefix.Length..].Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.GetFullPath(Path.Combine(uploadsRoot, relativePath));

            // Yol birlestirmesi uploads klasorunun disina cikiyorsa hicbir sey yapilmaz.
            if (!physicalPath.StartsWith(uploadsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                if (File.Exists(physicalPath))
                    File.Delete(physicalPath);
            }
            catch (IOException ex)
            {
                // Eski dosya silinemediyse islem yine basarili sayilir; yalnizca iz birakilir.
                _logger.LogWarning(ex, "Eski görsel silinemedi: {Path}", physicalPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Eski görsel silinemedi: {Path}", physicalPath);
            }
        }

        /// <summary>
        /// <c>wwwroot/uploads</c> klasorunun fiziksel yolu. Klasor yoksa olusturur.
        /// </summary>
        public string EnsureUploadsRoot()
        {
            var root = GetUploadsRoot();
            Directory.CreateDirectory(root);
            return root;
        }

        private string GetUploadsRoot()
        {
            var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;

            return Path.Combine(webRoot, "uploads");
        }

        private static string SanitizeFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
                return "misc";

            var cleaned = new string(folder.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').ToArray());
            return cleaned.Length == 0 ? "misc" : cleaned.ToLowerInvariant();
        }

        private static async Task<int> ReadExactlyAsync(Stream stream, byte[] buffer, CancellationToken ct)
        {
            var total = 0;
            while (total < buffer.Length)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(total, buffer.Length - total), ct);
                if (read == 0)
                    break;

                total += read;
            }

            return total;
        }

        private static bool HasKnownImageSignature(byte[] header, int length)
        {
            // JPEG: FF D8 FF
            if (length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return true;

            // PNG: 89 50 4E 47
            if (length >= 4 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                return true;

            // WEBP: "RIFF" (52 49 46 46) + 8. bayttan "WEBP" (57 45 42 50)
            if (length >= 12 &&
                header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
                return true;

            return false;
        }
    }
}
