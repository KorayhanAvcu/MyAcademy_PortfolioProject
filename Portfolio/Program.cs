using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Portfolio.Data.Context;
using Portfolio.Data.Entities;
using Portfolio.Infrastructure;
using Portfolio.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Loglar hem konsola hem gunluk donen dosyaya; 14 gun saklanir (logs/ .gitignore'da).
builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(context.HostingEnvironment.ContentRootPath, "logs", "portfolio-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        // Ayni dosyaya birden fazla host yazabilir (ornegin testlerde ikinci bir
        // host kuruldugunda); paylasimli kip dosya kilidi hatasini onler.
        shared: true));

// Uygulama sinirinin (2 MB) USTUNDE bir ham govde siniri: boylece buyuk bir dosya
// Kestrel'in ciplak 413'u yerine formda anlasilir bir hata mesaji olarak gorunur.
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 8 * 1024 * 1024);

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

// Gorsel yukleme: durumsuz, yalnizca IWebHostEnvironment'a bagli.
builder.Services.AddSingleton<LocalImageStorage>();
builder.Services.AddSingleton<IImageStorage>(sp => sp.GetRequiredService<LocalImageStorage>());

// Global AuthorizeFilter yerine fallback politika: nitelik yazmayi unutan bir
// controller boyle de kapali kalir, ama [AllowAnonymous] ve [Authorize] niteligi
// olan endpoint'ler kendi kurallarini kullanir.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireRole("Admin")
        .Build();
});

// Iletisim formu: IP basina 5 dakikada 3 istek. Gercek ziyaretci IP'si icin
// UseForwardedHeaders bu limitleyiciden ONCE calismak zorunda (yoksa tum tunel
// trafigi proxy'nin tek adresinde birikir).
builder.Services.AddRateLimiter(options =>
{
    // Varsayilan 503 degil: istemci "cok fazla istek" yanitini almali.
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("contact", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "bilinmeyen",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            }));
});

// View'lardaki silme islemleri token'i gizli alandan okuyup header'da gonderiyor.
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

builder.Services.AddControllersWithViews(options =>
{
    // Tum POST/PUT/PATCH/DELETE istekleri icin CSRF dogrulamasi.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

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

// Endpoint'e bagli politikalar ([EnableRateLimiting]) icin UseRouting'den sonra.
app.UseRateLimiter();

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

// Yuklenen gorsellerin klasoru acilista hazir olsun: ilk yukleme "klasor yok"
// hatasina dusmesin, statik dosya katmani da yolu tanisin.
app.Services.GetRequiredService<LocalImageStorage>().EnsureUploadsRoot();

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
