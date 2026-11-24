using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Infrastructure;

namespace Portfolio.Controllers
{
    /// <summary>
    /// Tum admin panel controller'larinin temeli: Admin rolu zorunlu ve yalnizca
    /// localhost'tan erisilebilir. Nitelik yazmayi unutan bir controller bu sinifa
    /// turetildigi anda korunur.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [LocalOnly]
    public abstract class AdminControllerBase : Controller
    {
        /// <summary>
        /// Gorsel alani icin ortak hata metni: dosya da URL de verilmediginde gosterilir.
        /// </summary>
        protected const string ImageRequiredMessage =
            "Bir görsel dosyası yükleyin veya görsel URL'si girin.";
    }
}
