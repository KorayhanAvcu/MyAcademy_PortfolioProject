using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using Portfolio.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Eksik baglanti string'i ilk sorguya kadar fark edilmesin: SqlClient'in anlasilmaz
// hatasi yerine acilista anlasilir bir hata (Ruling I).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection yapılandırılmamış");

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 10;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "PortfolioCookie";
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<AdminSeeder>();

// Global AuthorizeFilter yerine fallback politika: nitelik yazmayi unutan bir
// controller boyle de kapali kalir, ama [AllowAnonymous] ve [Authorize] niteligi
// olan endpoint'ler kendi kurallarini kullanir.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireRole("Admin")
        .Build();
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// 404 / 403 gibi govdesiz durum kodlari icin hata sayfasini yeniden calistir.
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseLocalOnlyEndpoints();
app.UseAuthentication();
app.UseAuthorization();

// "/Home/Error/404" yolundaki segment statusCode'a baglanmali; varsayilan rota
// onu "id" olarak baglardi.
app.MapControllerRoute(
    name: "error",
    pattern: "Home/Error/{statusCode?}",
    defaults: new { controller = "Home", action = "Error" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}/{id?}");

// Sema ve tek admin kullanicisi acilista hazirlanir.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
    await seeder.RunAsync();
}

app.Run();

public partial class Program { }
