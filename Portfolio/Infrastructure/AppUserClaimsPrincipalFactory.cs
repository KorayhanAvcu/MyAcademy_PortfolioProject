using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Portfolio.Data.Entities;

namespace Portfolio.Infrastructure
{
    /// <summary>
    /// Admin panelinin ihtiyac duydugu kullanici bilgilerini (ad, profil resmi)
    /// kimlik cerezine claim olarak yazar; boylece Session'a gerek kalmaz.
    /// </summary>
    public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, IdentityRole>
    {
        public AppUserClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Bos degerler icin claim yazilmaz: Claim degeri null olamaz.
            if (!string.IsNullOrEmpty(user.FullName))
                identity.AddClaim(new Claim("fullname", user.FullName));

            if (!string.IsNullOrEmpty(user.ImageUrl))
                identity.AddClaim(new Claim("imageUrl", user.ImageUrl));

            return identity;
        }
    }
}
