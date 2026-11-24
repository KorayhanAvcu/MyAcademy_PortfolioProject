using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Entities;
using Portfolio.Infrastructure;
using Portfolio.Models;

namespace Portfolio.Controllers
{
    // NOT: [AllowAnonymous] bilerek controller duzeyinde DEGIL, action duzeyinde.
    // Authorization middleware endpoint uzerinde bir IAllowAnonymous metadata'si
    // gorurse yetki degerlendirmesini tumuyle atlar; controller duzeyinde kalirsa
    // ChangePassword'un [Authorize] niteligi etkisiz kalir ve sayfa anonim acilir.
    // Giris sayfasi yine anonim erisilebilir — ama yalnizca localhost'tan.
    [LocalOnly]
    public class AuthController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, isPersistent: false, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty, "Hesabınız geçici olarak kilitlendi. 15 dakika sonra tekrar deneyin.");
                return View(model);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı!");
                return View(model);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Default");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordVM());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ChangePasswordAsync(
                user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            // Ruling DD: claim'ler oturum acilisinda cereze kopyalaniyor. Yenilenmezse
            // cerez eski guvenlik damgasini tasir ve kullanici beklenmedik sekilde
            // oturumdan dusebilir.
            await _signInManager.RefreshSignInAsync(user);

            TempData["Bilgi"] = "Şifreniz başarıyla güncellendi.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
