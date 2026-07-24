using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Data.Context;
using Portfolio.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Portfolio.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var admin = _context.Admins.FirstOrDefault(x=>x.UserName == model.UserName && x.Password == model.Password);

            if(!ModelState.IsValid)
            {
                return View(model);
            }
            if(admin == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı!");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, admin.UserName),
                new Claim("fullname",admin.FullName),
                new Claim("imageUrl", admin.ImageUrl)
            };
            var claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
            var authProps = new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
                IsPersistent = false
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(claimsIdentity),authProps);
            HttpContext.Session.SetString("fullname", admin.FullName);
            HttpContext.Session.SetString("imageUrl", admin.ImageUrl);

            return RedirectToAction("Index","Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("fullname");
            HttpContext.Session.Remove("imageUrl");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Default");

        }
    }
}
