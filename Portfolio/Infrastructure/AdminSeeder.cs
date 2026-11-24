using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Entities;

namespace Portfolio.Infrastructure
{
    public enum SeedOutcome
    {
        Created,
        SkippedUserExists
    }

    /// <summary>
    /// Tek admin kullanicisini acilista olusturur. Kayit (register) yolu yok;
    /// <see cref="UserManager{TUser}.CreateAsync(TUser, string)"/> yalnizca buradan cagrilir.
    /// </summary>
    public class AdminSeeder
    {
        public const string AdminEmail = "korayhanavcu@gmail.com";
        public const string AdminRole = "Admin";
        public const string PasswordConfigKey = "ADMIN_SEED_PASSWORD";

        private readonly UserManager<AppUser> _users;
        private readonly RoleManager<IdentityRole> _roles;
        private readonly IConfiguration _config;

        public AdminSeeder(UserManager<AppUser> users, RoleManager<IdentityRole> roles, IConfiguration config)
        {
            _users = users;
            _roles = roles;
            _config = config;
        }

        public async Task<SeedOutcome> RunAsync(CancellationToken ct = default)
        {
            if (!await _roles.RoleExistsAsync(AdminRole))
            {
                var roleResult = await _roles.CreateAsync(new IdentityRole(AdminRole));
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException(
                        $"'{AdminRole}' rolu olusturulamadi: {Describe(roleResult)}");
            }

            // Idempotent: herhangi bir kullanici varsa hicbir sey yapilmaz ve sifre HIC okunmaz.
            if (await _users.Users.AnyAsync(ct))
                return SeedOutcome.SkippedUserExists;

            var password = _config[PasswordConfigKey];
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException(
                    $"Admin kullanicisi yok ve '{PasswordConfigKey}' yapilandirilmamis. " +
                    "Uygulama admin olmadan baslatilamaz.");

            var user = new AppUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FullName = "Korayhan Avcu"
            };

            var createResult = await _users.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException(
                    $"Admin kullanicisi olusturulamadi: {Describe(createResult)}");

            var addToRoleResult = await _users.AddToRoleAsync(user, AdminRole);
            if (!addToRoleResult.Succeeded)
                throw new InvalidOperationException(
                    $"Admin kullanicisi '{AdminRole}' rolune eklenemedi: {Describe(addToRoleResult)}");

            return SeedOutcome.Created;
        }

        private static string Describe(IdentityResult result) =>
            string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
    }
}
