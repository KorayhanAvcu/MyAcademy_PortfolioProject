namespace Portfolio.Services
{
    /// <summary>
    /// Yuklenen gorselin kaydedilme sonucu. Basarisizlikta <see cref="ErrorMessage"/>
    /// dogrudan forma yazilabilecek, kullaniciya donuk bir metindir.
    /// </summary>
    public sealed record ImageSaveResult(bool Success, string? RelativeUrl, string? ErrorMessage);

    /// <summary>
    /// Admin panelinden gelen gorsel dosyalarini saklar.
    /// </summary>
    public interface IImageStorage
    {
        /// <summary>
        /// Dosyayi dogrular ve <c>/uploads/&lt;folder&gt;/&lt;guid&gt;&lt;uzanti&gt;</c>
        /// yoluna yazar. Istemcinin dosya adi hicbir zaman kullanilmaz.
        /// </summary>
        Task<ImageSaveResult> SaveAsync(IFormFile file, string folder, CancellationToken ct = default);

        /// <summary>
        /// Yalnizca <c>/uploads/</c> ile baslayan yollari siler; harici URL'lere dokunmaz.
        /// </summary>
        void Delete(string? relativeUrl);
    }
}
