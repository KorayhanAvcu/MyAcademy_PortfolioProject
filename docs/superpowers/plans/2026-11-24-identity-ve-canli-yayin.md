# Identity Geçişi, Güvenlik Düzeltmeleri ve Canlı Yayın — Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Portfolyo sitesini ASP.NET Core Identity'ye taşımak, canlıya çıkışı engelleyen güvenlik açıklarını kapatmak, görsel yüklemeyi eklemek ve siteyi kullanıcının kendi makinesinden sıfır maliyetle yayına almak.

**Architecture:** Mevcut tek projeli ASP.NET Core 8 MVC yapısı korunuyor. Kimlik doğrulama `Admin` entity'sinden Identity'ye taşınıyor (`AppUser : IdentityUser`), `AppDbContext` `IdentityDbContext`'e dönüyor. Admin controller'ları ortak bir `AdminControllerBase`'den türeyerek hem rol hem "yalnızca localhost" kısıtını tek yerden alıyor; buna ek olarak global bir `FallbackPolicy` unutulan nitelikleri yakalıyor. Görseller `IImageStorage` arkasında dosya sistemine yazılıyor, `ImageUrl` kolonu hem göreli yol hem harici URL kabul edecek şekilde korunuyor.

**Tech Stack:** .NET 8 (net8.0), ASP.NET Core MVC, EF Core 8 + SQL Server Express (Windows auth), ASP.NET Core Identity, xUnit + `Microsoft.AspNetCore.Mvc.Testing`, Serilog, Tailwind CLI, GitHub Actions (self-hosted runner), Cloudflare Tunnel.

**Spec:** `docs/superpowers/specs/2026-11-24-identity-ve-canli-yayin-design.md`

## Global Constraints

- Hedef framework `net8.0`. Yükseltme yapılmayacak.
- EF sağlayıcısı `Microsoft.EntityFrameworkCore.SqlServer` kalacak. PostgreSQL'e geçilmeyecek.
- Hiçbir şifre, connection string veya gizli değer commit edilmeyecek — `appsettings.json` dahil.
- `Admins` dışında hiçbir tablo düşürülmeyecek; mevcut içerik korunacak (2 proje, 18 yetenek, 8 referans, 2 deneyim, 2 eğitim, 3 hizmet, 1 banner, 1 hakkında, 133 mesaj).
- Public tarafın görünümü değişmeyecek.
- Şifre politikası: en az 10 karakter, rakam + büyük harf + küçük harf zorunlu, özel karakter zorunlu değil.
- Hesap kilitleme: 5 başarısız deneme → 15 dakika.
- Yüklenen görseller: izinli uzantılar `.jpg/.jpeg/.png/.webp`, azami 2 MB.
- Tek admin: `korayhanavcu@gmail.com`. Kayıt/register yolu hiç yazılmayacak.
- Her commit mesajı şu satırla bitecek:
  `Co-Authored-By: Claude Opus 5 <noreply@anthropic.com>`
- Test veritabanı adı `PortfolioDb_Test`; testler onu migration ile kurup sonunda düşürecek.

## Review Focus

Spec'in ima ettiği ama hiçbir görevin testinin kendiliğinden kapsamadığı, kullanıcıyı en çok ısıracak beş durum. Her biri için test, kodu barındıran göreve eklendi:

1. **İçerik tabloları boşken public anasayfa.** View component'ler ilk kaydı varsayıyorsa `/` 500 dönüyor. Referanslar silineceği için bu senaryo kesin oluşacak. → Görev 11, Adım 1.
2. **2 MB üstü dosya yüklemesi.** Kullanıcı form üzerinde anlaşılır bir hata görmeli, Kestrel'in ham 413 sayfasını değil. → Görev 7, Adım 7.
3. **Büyük harfli uzantı (`.JPG`, `.PNG`).** Geçerli dosya reddedilmemeli. → Görev 7, Adım 5.
4. **Düzenleme formunda dosya seçilmemesi.** Mevcut görsel (yüklenmiş ya da harici URL) silinmemeli, korunmalı. → Görev 7, Adım 9.
5. **Oturum süresi dolmuşken form gönderimi.** Antiforgery hatası ziyaretçiye 400 olarak düşmemeli, giriş sayfasına yönlenmeli. → Görev 6, Adım 7.

---

## Dosya yapısı

**Yeni dosyalar:**

| Dosya | Sorumluluk |
|---|---|
| `Portfolio/Data/Entities/AppUser.cs` | Identity kullanıcısı + `FullName`, `ImageUrl` |
| `Portfolio/Infrastructure/AppUserClaimsPrincipalFactory.cs` | `fullname` / `imageUrl` claim'lerini üretir |
| `Portfolio/Infrastructure/AdminSeeder.cs` | Tek admin'i oluşturur, idempotent |
| `Portfolio/Infrastructure/LocalOnlyAttribute.cs` | Localhost dışı istekleri 404'e çevirir |
| `Portfolio/Controllers/AdminControllerBase.cs` | Admin controller'larının ortak kısıtları |
| `Portfolio/Services/IImageStorage.cs` | Görsel kaydetme/silme sözleşmesi |
| `Portfolio/Services/LocalImageStorage.cs` | Dosya sistemine yazan uygulama |
| `Portfolio/Models/ChangePasswordVM.cs` | Şifre değiştirme formu |
| `Portfolio/Models/ContactFormVM.cs` | İletişim formu + honeypot |
| `Portfolio/tailwind/*.config.js` | Üç palet için Tailwind konfigürasyonları |
| `Portfolio.Tests/*` | Test projesi |
| `.github/workflows/ci.yml` | Build + test + deploy |
| `scripts/backup-db.ps1` | Günlük yedek |

**Değişen dosyalar:** `Program.cs`, `Data/Context/AppDbContext.cs`, `Controllers/AuthController.cs`, 14 admin controller (base sınıf), 12 silme action'ı + ilgili index view'ları, `ViewComponents/AdminLayout/_AdminLayoutSidebarViewComponent.cs`, `Views/Auth/Login.cshtml`, `Views/Shared/Components/_DefaultHead/Default.cshtml`, `Views/AdminLayout/Layout.cshtml`, `Portfolio.csproj`, `appsettings*.json`, `.gitignore`.

**Silinen dosyalar:** `Data/Entities/Admin.cs`, `Views/AdminLayout/HeadPartial.cshtml`, `AdminLayoutController.HeadPartial()`, `wwwroot/stitch_portfolio.zip`, `wwwroot/stitch_modern_portfolio_admin_login.zip`, `wwwroot/stitch_portfolio/`, `wwwroot/stitch_modern_portfolio_admin_login/`, `wwwroot/AdminTemplate/`.

---

## Görev 1: Config'i koddan çıkar

**Files:**
- Modify: `Portfolio/Data/Context/AppDbContext.cs` (`OnConfiguring` silinir)
- Modify: `Portfolio/Program.cs:23` (`AddDbContext<AppDbContext>()`)
- Modify: `Portfolio/appsettings.json`, `Portfolio/appsettings.Development.json`
- Modify: `.gitignore`

**Interfaces:**
- Consumes: —
- Produces: `ConnectionStrings:DefaultConnection` ve `Site:BaseUrl` config anahtarları; sonraki görevlerin tamamı bunlara dayanır.

- [ ] **Adım 1: `OnConfiguring` metodunu `AppDbContext`'ten sil.** Sınıf yalnızca `DbSet`'leri ve `public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)` constructor'ını taşıyacak.

- [ ] **Adım 2: `Program.cs`'te bağlantıyı config'den ver.**
  `builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")))`

- [ ] **Adım 3: `appsettings.Development.json`'a geliştirme bağlantısını ve site adresini ekle.**
  `ConnectionStrings:DefaultConnection` = `Server=KORAYHAN\SQLEXPRESS;Database=PortfolioDb;Integrated Security=true;TrustServerCertificate=true`
  `Site:BaseUrl` = `https://localhost:7000`
  `appsettings.json`'a yalnızca boş `"ConnectionStrings": { "DefaultConnection": "" }` ve `"Site": { "BaseUrl": "" }` iskeletleri girecek — gerçek değerler environment variable'dan gelecek.

- [ ] **Adım 4: `.gitignore`'a ekle:** `appsettings.Production.json`, `logs/`, `Portfolio/wwwroot/uploads/`.

- [ ] **Adım 5: Uygulamayı çalıştırıp anasayfanın açıldığını doğrula.**
  Çalıştır: `dotnet run --project Portfolio`
  Beklenen: `/` 200 dönüyor, veritabanı içeriği görünüyor.

- [ ] **Adım 6: Commit.**
```bash
git add Portfolio/Data/Context/AppDbContext.cs Portfolio/Program.cs Portfolio/appsettings*.json .gitignore
git commit -m "refactor: connection string'i koddan config'e taşı"
```

---

## Görev 2: Test projesi ve ilk duman testi

**Files:**
- Create: `Portfolio.Tests/Portfolio.Tests.csproj`
- Create: `Portfolio.Tests/PortfolioAppFactory.cs`
- Create: `Portfolio.Tests/PublicSiteTests.cs`
- Modify: `Portfolio/Program.cs` (dosya sonuna `public partial class Program { }`)
- Modify: `MyAcademy_PortfolioProject.sln`

**Interfaces:**
- Consumes: Görev 1'in config anahtarları.
- Produces: `PortfolioAppFactory : WebApplicationFactory<Program>, IAsyncLifetime` — test veritabanını migration ile kurar, `DisposeAsync`'te düşürür. `public HttpClient CreateAnonymousClient()` ve `public IServiceProvider Services` üzerinden servis erişimi. Sonraki tüm entegrasyon testleri bunu kullanır.

- [ ] **Adım 1: Test projesini oluştur ve solution'a ekle.**
```bash
dotnet new xunit -o Portfolio.Tests -f net8.0
dotnet add Portfolio.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add Portfolio.Tests reference Portfolio/Portfolio.csproj
dotnet sln add Portfolio.Tests/Portfolio.Tests.csproj
```

- [ ] **Adım 2: `Program.cs` dosyasının en sonuna `public partial class Program { }` ekle.** `WebApplicationFactory<Program>` top-level statement kullanan bir programı ancak böyle görebilir.

- [ ] **Adım 3: `PortfolioAppFactory`'yi yaz.** `ConfigureWebHost` içinde `ConfigureAppConfiguration` ile `ConnectionStrings:DefaultConnection` değerini `PortfolioDb_Test`'e, `ADMIN_SEED_PASSWORD` değerini `Test!Parola123` değerine çeker. `InitializeAsync` migration uygular, `DisposeAsync` veritabanını düşürür.

- [ ] **Adım 4: Başarısız olacak testi yaz.**
```csharp
public class PublicSiteTests : IClassFixture<PortfolioAppFactory>
{
    private readonly PortfolioAppFactory _factory;
    public PublicSiteTests(PortfolioAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Anasayfa_anonim_olarak_200_doner()
    {
        var client = _factory.CreateAnonymousClient();
        var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

- [ ] **Adım 5: Testi çalıştırıp başarısız olduğunu gör.**
  Çalıştır: `dotnet test --filter Anasayfa_anonim_olarak_200_doner`
  Beklenen: FAIL — 307 dönüyor. `DefaultController` zaten `[AllowAnonymous]` olduğu için yetki sorun değil; başarısızlığın nedeni `UseHttpsRedirection`'ın test istemcisinin `http://localhost` adresini HTTPS'e yönlendirmesi.

- [ ] **Adım 6: `CreateAnonymousClient()`'ın `BaseAddress`'ini `https://localhost/` yap.** `TestServer` gerçek TLS yapmaz ama `Request.IsHttps` true olur ve `UseHttpsRedirection` isteği geçirir. Bu, üretim davranışını değiştirmeden testi doğru ortama oturtur.

- [ ] **Adım 7: Testi çalıştırıp geçtiğini gör.**
  Çalıştır: `dotnet test --filter Anasayfa_anonim_olarak_200_doner`
  Beklenen: PASS

- [ ] **Adım 8: Commit.**
```bash
git add Portfolio.Tests MyAcademy_PortfolioProject.sln Portfolio/Program.cs
git commit -m "test: xUnit test projesi ve anasayfa duman testi"
```

---

## Görev 3: Identity'ye geçiş

**Files:**
- Create: `Portfolio/Data/Entities/AppUser.cs`
- Create: `Portfolio/Infrastructure/AppUserClaimsPrincipalFactory.cs`
- Delete: `Portfolio/Data/Entities/Admin.cs`
- Modify: `Portfolio/Data/Context/AppDbContext.cs`
- Modify: `Portfolio/Program.cs`
- Modify: `Portfolio/ViewComponents/AdminLayout/_AdminLayoutSidebarViewComponent.cs`
- Modify: `Portfolio/Portfolio.csproj`
- Create: `Portfolio/Migrations/<timestamp>_mig_identity.cs` (üretilecek)
- Test: `Portfolio.Tests/IdentitySchemaTests.cs`

**Interfaces:**
- Consumes: Görev 2'nin `PortfolioAppFactory`'si.
- Produces:
  - `Portfolio.Data.Entities.AppUser : IdentityUser` — `public string FullName { get; set; }`, `public string? ImageUrl { get; set; }`
  - `Portfolio.Data.Context.AppDbContext : IdentityDbContext<AppUser, IdentityRole, string>`
  - Claim adları: `"fullname"`, `"imageUrl"` (küçük harfli; eski session anahtarlarının yerini alır)

- [ ] **Adım 1: Identity paketini ekle.**
```bash
dotnet add Portfolio package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.28
```

- [ ] **Adım 2: Başarısız olacak testi yaz.**
```csharp
[Fact]
public async Task Identity_tablolari_var_icerik_tablolari_korunmus_Admin_kalkmis()
{
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Identity tablosu gerçekten sorgulanabiliyor (yoksa SqlException atar)
    Assert.Equal(0, await db.Users.CountAsync());

    // 13 içerik DbSet'i hâlâ modelde
    var names = db.Model.GetEntityTypes().Select(t => t.ClrType.Name).ToList();
    foreach (var n in new[] { "About", "Banner", "Category", "ContactInfo", "Education",
                              "Experience", "Project", "Service", "Skill", "TechStack",
                              "Testimonial", "ProjectTechStack", "UserMessage" })
        Assert.Contains(n, names);

    // Admin entity'si kalkmış
    Assert.DoesNotContain("Admin", names);
}
```

- [ ] **Adım 3: Testi çalıştırıp başarısız olduğunu gör.**
  Çalıştır: `dotnet test --filter Identity_tablolari_olusuyor`
  Beklenen: FAIL — `db.Users` derlenmiyor.

- [ ] **Adım 4: `AppUser`'ı yaz ve `Admin.cs`'i sil.**

- [ ] **Adım 5: `AppDbContext`'i `IdentityDbContext<AppUser, IdentityRole, string>`'ten türet.** `DbSet<Admin> Admins` satırı kaldırılacak, diğer 13 `DbSet` korunacak. `OnModelCreating`'de `base.OnModelCreating(builder)` çağrısı şart.

- [ ] **Adım 6: `AppUserClaimsPrincipalFactory`'yi yaz.** `UserClaimsPrincipalFactory<AppUser, IdentityRole>`'dan türer, `GenerateClaimsAsync` override'ında `fullname` ve `imageUrl` claim'lerini ekler.

- [ ] **Adım 7: `Program.cs`'i güncelle.**
  - `AddIdentity<AppUser, IdentityRole>` ile şifre politikası (min 10, rakam/büyük/küçük zorunlu, özel karakter değil) ve kilit (5 deneme / 15 dakika) ayarlanır; `.AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders()`.
  - `AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()` zincire eklenir.
  - `ConfigureApplicationCookie` ile çerez adı `PortfolioCookie`, `LoginPath = "/Auth/Login"`, `LogoutPath = "/Auth/Logout"`, `ExpireTimeSpan = 30 dakika`, `SlidingExpiration = true`.
  - Mevcut elle yazılmış `AddAuthentication(...).AddCookie(...)` bloğu kaldırılır.
  - **`AddSession` çağrısı ve `app.UseSession()` satırı kaldırılır.**

- [ ] **Adım 8: Sidebar view component'ini claim okuyacak şekilde değiştir.** `HttpContext.Session.GetString("fullName")` yerine `UserClaimsPrincipal.FindFirst("fullname")?.Value`; `imageUrl` için aynısı. (Bu, mevcut `fullName`/`fullname` harf hatasını da ortadan kaldırıyor.)

- [ ] **Adım 9: Migration üret ve uygula.**
```bash
dotnet ef migrations add mig_identity --project Portfolio
dotnet ef database update --project Portfolio
```
  Migration dosyasını **uygulamadan önce oku**: yalnızca `AspNet*` tablolarının eklendiğini ve `Admins` dışında tablo düşürülmediğini doğrula. Başka bir tablo düşülüyorsa migration'ı sil ve düzelt.

- [ ] **Adım 10: Testleri çalıştırıp geçtiğini gör.**
  Çalıştır: `dotnet test`
  Beklenen: PASS (Görev 2'nin testi dahil)

- [ ] **Adım 11: Commit.**
```bash
git add Portfolio Portfolio.Tests
git commit -m "feat: kimlik dogrulamayi ASP.NET Core Identity'ye tasi"
```

---

## Görev 4: Tek admin'in seed edilmesi ve giriş

**Files:**
- Create: `Portfolio/Infrastructure/AdminSeeder.cs`
- Create: `Portfolio/Models/ChangePasswordVM.cs`
- Modify: `Portfolio/Models/LoginViewModel.cs`
- Modify: `Portfolio/Controllers/AuthController.cs`
- Modify: `Portfolio/Views/Auth/Login.cshtml`
- Create: `Portfolio/Views/Auth/ChangePassword.cshtml`
- Modify: `Portfolio/Program.cs`
- Test: `Portfolio.Tests/AdminSeederTests.cs`, `Portfolio.Tests/LoginTests.cs`

**Interfaces:**
- Consumes: `AppUser`, `AppDbContext` (Görev 3).
- Produces:
  - `Portfolio.Infrastructure.AdminSeeder` — `public AdminSeeder(UserManager<AppUser> users, RoleManager<IdentityRole> roles, IConfiguration config)`, `public Task<SeedOutcome> RunAsync(CancellationToken ct = default)`
  - `public enum SeedOutcome { Created, SkippedUserExists }`
  - Kullanıcı yokken şifre yoksa `InvalidOperationException` fırlatır.
  - Sabitler: e-posta `korayhanavcu@gmail.com`, rol adı `Admin`, config anahtarı `ADMIN_SEED_PASSWORD`.
  - `LoginViewModel` artık `Email` (+ `[EmailAddress]`) ve `Password` taşır; `UserName` kaldırıldı.

- [ ] **Adım 1: Başarısız olacak testleri yaz.**
```csharp
[Fact] public async Task Kullanici_yokken_seeder_admini_olusturur_ve_role_ekler() { /* RunAsync() -> SeedOutcome.Created; UserManager.FindByEmailAsync(...) null degil; IsInRoleAsync(user,"Admin") true */ }
[Fact] public async Task Kullanici_varken_seeder_ikinci_kullanici_olusturmaz() { /* iki kez RunAsync(); ikincisi SeedOutcome.SkippedUserExists; db.Users.Count() == 1 */ }
[Fact] public async Task Kullanici_ve_sifre_yokken_seeder_hata_firlatir() { /* ADMIN_SEED_PASSWORD bos -> InvalidOperationException */ }
```

- [ ] **Adım 2: Testleri çalıştırıp başarısız olduklarını gör.**
  Çalıştır: `dotnet test --filter AdminSeederTests`
  Beklenen: FAIL — `AdminSeeder` yok.

- [ ] **Adım 3: `AdminSeeder`'ı yaz.** `Admin` rolü yoksa oluşturur; `users.Users.AnyAsync()` true ise `SkippedUserExists` döner (şifreyi hiç okumaz); false ise config'den şifreyi okur, boşsa `InvalidOperationException` fırlatır, doluysa kullanıcıyı `EmailConfirmed = true` ile oluşturup role ekler.

- [ ] **Adım 4: `Program.cs`'te `app.Run()` öncesinde seeder'ı çalıştır.** Bir scope açıp `AdminSeeder`'ı çözer ve `RunAsync()` çağırır. `db.Database.Migrate()` çağrısı bu satırın **hemen öncesine** gelir.

- [ ] **Adım 5: Seeder testlerini çalıştırıp geçtiklerini gör.**
  Çalıştır: `dotnet test --filter AdminSeederTests`
  Beklenen: PASS

- [ ] **Adım 6: Giriş testlerini yaz.**
```csharp
[Fact] public async Task Dogru_sifreyle_giris_dashboarda_yonlendirir() { /* POST /Auth/Login -> 302, Location "/Dashboard" icerir */ }
[Fact] public async Task Bes_yanlis_denemeden_sonra_hesap_kilitlenir() { /* 5x yanlis POST; 6. istek DOGRU sifreyle de giris yaptirmaz */ }
```
  Her iki test de antiforgery token'ı giriş sayfasının HTML'inden çıkarıp göndermek zorunda — `PortfolioAppFactory`'ye `Task<string> GetAntiforgeryTokenAsync(HttpClient client, string path)` yardımcısı eklenecek.

- [ ] **Adım 7: `AuthController`'ı `SignInManager` kullanacak şekilde yeniden yaz.** `PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: true)`. Elle claim kurma, `HttpContext.SignInAsync` ve session yazma satırları kaldırılır. `Logout` `SignOutAsync()` çağırır. Kilitli hesap için ayrı hata mesajı: `"Hesabınız geçici olarak kilitlendi. 15 dakika sonra tekrar deneyin."`

- [ ] **Adım 8: `LoginViewModel`'i ve `Login.cshtml`'i e-posta alanına çevir.** Görünüm değişmeyecek, yalnızca alan adı ve etiket (`Kullanıcı adı` → `E-posta`).

- [ ] **Adım 9: Giriş testlerini çalıştırıp geçtiklerini gör.**
  Çalıştır: `dotnet test --filter LoginTests`
  Beklenen: PASS

- [ ] **Adım 10: "Şifre Değiştir" sayfasını ekle.** `ChangePasswordVM` (`CurrentPassword`, `NewPassword`, `ConfirmPassword` + `[Compare]`), `AuthController.ChangePassword` GET/POST, `UserManager.ChangePasswordAsync`. Başarıda `TempData` ile bilgi mesajı ve `/Dashboard`'a yönlendirme.

- [ ] **Adım 11: Tüm testleri çalıştır.**
  Çalıştır: `dotnet test`
  Beklenen: PASS

- [ ] **Adım 12: Commit.**
```bash
git add Portfolio Portfolio.Tests
git commit -m "feat: tek admin seed'i, Identity girisi ve sifre degistirme"
```

---

## Görev 5: Yetkilendirme, localhost kısıtı ve hata sayfaları

**Files:**
- Create: `Portfolio/Infrastructure/LocalOnlyAttribute.cs`
- Create: `Portfolio/Controllers/AdminControllerBase.cs`
- Modify: 14 admin controller (`About`, `AdminLayout`, `Banner`, `Category`, `ContactInfo`, `Dashboard`, `Education`, `Experience`, `Project`, `ProjectTechStacks`, `Service`, `Skill`, `Testimonial`, `UserMessage`) — `Controller` yerine `AdminControllerBase`
- Modify: `Portfolio/Controllers/AuthController.cs` (`[LocalOnly]` eklenir)
- Modify: `Portfolio/Controllers/HomeController.cs` (`[AllowAnonymous]`)
- Modify: `Portfolio/Program.cs`
- Modify: `Portfolio/Views/Shared/Error.cshtml`
- Test: `Portfolio.Tests/AuthorizationTests.cs`

**Interfaces:**
- Consumes: Identity rolleri (Görev 3), `Admin` rolü (Görev 4).
- Produces:
  - `Portfolio.Infrastructure.LocalOnlyAttribute : Attribute, IAuthorizationFilter` — `OnAuthorization` içinde `context.HttpContext.Connection.RemoteIpAddress` loopback değilse `context.Result = new NotFoundResult()`.
  - `Portfolio.Controllers.AdminControllerBase : Controller` — `[Authorize(Roles = "Admin")]` ve `[LocalOnly]` taşır.
  - `PortfolioAppFactory.CreateRemoteClient()` — `ConfigureTestServices` ile eklenen bir middleware `HttpContext.Connection.RemoteIpAddress`'i `203.0.113.5` yapar. **Header değil, doğrudan bağlantı adresi** — böylece bu test Görev 9'un forwarded-headers ayarına bağımlı olmaz.

- [ ] **Adım 1: Başarısız olacak testleri yaz.**
```csharp
[Fact] public async Task Dashboard_anonim_olarak_giris_sayfasina_yonlendirir() { /* GET /Dashboard -> 302, Location "/Auth/Login" icerir */ }
[Fact] public async Task Admin_yollari_localhost_disindan_404_doner() { /* CreateRemoteClient().GetAsync("/Dashboard") -> 404 */ }
[Fact] public async Task Hata_sayfasi_anonim_olarak_erisilebilir() { /* GET /Home/Error -> 200 */ }
[Fact] public async Task Bilinmeyen_yol_404_sayfasi_doner() { /* GET /olmayan-bir-yol -> 404 ve govde bos degil */ }
```

- [ ] **Adım 2: Testleri çalıştırıp başarısız olduklarını gör.**
  Çalıştır: `dotnet test --filter AuthorizationTests`
  Beklenen: FAIL — `/Home/Error` 302 dönüyor, uzak istemci 200 alıyor.

- [ ] **Adım 3: `LocalOnlyAttribute`'u yaz.**

- [ ] **Adım 4: `AdminControllerBase`'i yaz ve 14 admin controller'ı ondan türet.**

- [ ] **Adım 5: `AuthController`'a `[LocalOnly]` ekle** (`[AllowAnonymous]` korunur — giriş sayfası localhost'tan anonim erişilebilir olmalı).

- [ ] **Adım 6: `HomeController`'a `[AllowAnonymous]` ekle.**

- [ ] **Adım 7: `Program.cs`'te global filtreyi politikaya çevir.** `options.Filters.Add(new AuthorizeFilter())` satırı kaldırılır; yerine `AddAuthorization(o => o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireRole("Admin").Build())`. Nitelik yazmayı unutan bir controller böyle yine kapalı kalır.

- [ ] **Adım 8: 404 sayfasını bağla.** `app.UseStatusCodePagesWithReExecute("/Home/Error/{0}")`; `HomeController.Error` isteğe bağlı `int? statusCode` parametresi alır ve `Error.cshtml` durum koduna göre metin gösterir.

- [ ] **Adım 9: Testleri çalıştırıp geçtiklerini gör.**
  Çalıştır: `dotnet test`
  Beklenen: PASS

- [ ] **Adım 10: Commit.**
```bash
git add Portfolio Portfolio.Tests
git commit -m "feat: rol tabanli yetki, localhost kisiti ve hata sayfalari"
```

---

## Görev 6: CSRF koruması ve 12 GET silme action'ı

**Files:**
- Modify: `Portfolio/Program.cs`
- Modify: 12 controller'daki silme action'ları
- Modify: ~12 admin index view'ındaki silme çağrıları
- Test: `Portfolio.Tests/CsrfTests.cs`

**Interfaces:**
- Consumes: `AdminControllerBase` (Görev 5).
- Produces: Tüm silme action'ları `[HttpPost]`; view'lardaki `window.location.href = "/X/DeleteX/" + id` çağrıları yerine antiforgery token'lı `fetch` POST.

- [ ] **Adım 1: Başarısız olacak testleri yaz.**
```csharp
[Fact] public async Task Token_olmayan_post_400_doner() { /* giris yapilmis istemci, POST /Project/CreateProject token'siz -> 400 */ }
[Fact] public async Task Silme_action_i_get_ile_cagrilamaz() { /* giris yapilmis istemci, GET /Project/DeleteProject/1 -> 404 veya 405 */ }
```

- [ ] **Adım 2: Testleri çalıştırıp başarısız olduklarını gör.**
  Çalıştır: `dotnet test --filter CsrfTests`
  Beklenen: FAIL — token'sız POST 302 dönüyor, GET silme 302 dönüyor (silme gerçekleşiyor).

- [ ] **Adım 3: `Program.cs`'te global antiforgery doğrulamasını aç.**
  `options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())`

- [ ] **Adım 4: 12 silme action'ına `[HttpPost]` ekle.** Liste: `AboutController.DeleteAbout`, `BannerController.DeleteBanner`, `CategoryController.DeleteCategory`, `ContactInfoController.DeleteContactInfo`, `EducationController.DeleteEducation`, `ExperienceController.DeleteExperience`, `ProjectController.DeleteProject`, `ProjectTechStacksController.Delete`, `ServiceController.DeleteService`, `SkillController.DeleteSkill`, `TestimonialController.DeleteTestimonial`, `UserMessageController.DeleteUserMessage`. Her birinde `Find(id)` null dönerse `NotFound()` döndürülecek (şu an null referans hatası veriyor).

- [ ] **Adım 5: View'lardaki silme çağrılarını POST'a çevir.** Mevcut kalıp SweetAlert onayından sonra `window.location.href` kullanıyor; yerine aynı onayın `then` bloğunda `fetch(url, { method: 'POST', headers: { 'RequestVerificationToken': token } })` ve başarıda `location.reload()`. Token, layout'a eklenecek gizli bir `@Html.AntiForgeryToken()` alanından okunacak. `Program.cs`'te `AddAntiforgery(o => o.HeaderName = "RequestVerificationToken")` ayarlanacak.

- [ ] **Adım 6: Testleri çalıştırıp geçtiklerini gör.**
  Çalıştır: `dotnet test --filter CsrfTests`
  Beklenen: PASS

- [ ] **Adım 7: Review Focus #5 — süresi dolmuş oturumla gönderim testini yaz ve geçir.**
```csharp
[Fact] public async Task Suresi_dolmus_cerezle_post_giris_sayfasina_yonlendirir() { /* gecersiz PortfolioCookie ile POST -> 302, Location "/Auth/Login" */ }
```
  Geçmesi için `AddAntiforgery` yanında `ConfigureApplicationCookie`'nin yönlendirmesi yeterli olmazsa, antiforgery hatasını yakalayıp giriş sayfasına yönlendiren bir `IAsyncExceptionFilter` eklenecek.

- [ ] **Adım 8: Admin panelinde her varlık için bir silme işlemini elle dene.** Onay kutusu çıkıyor, kayıt siliniyor, sayfa yenileniyor.

- [ ] **Adım 9: Commit.**
```bash
git add Portfolio Portfolio.Tests
git commit -m "feat: global CSRF korumasi ve silme action'larini POST'a cevir"
```

---

## Görev 7: Görsel yükleme

**Files:**
- Create: `Portfolio/Services/IImageStorage.cs`, `Portfolio/Services/LocalImageStorage.cs`
- Modify: `Portfolio/Controllers/{About,Banner,Project}Controller.cs`
- Modify: `Portfolio/Views/{About,Banner,Project}/Create*.cshtml` ve `Update*.cshtml`
- Modify: `Portfolio/Program.cs`
- Test: `Portfolio.Tests/ImageUploadTests.cs`

**Interfaces:**
- Consumes: `AdminControllerBase` (Görev 5), antiforgery (Görev 6).
- Produces:
  - `public interface IImageStorage { Task<ImageSaveResult> SaveAsync(IFormFile file, string folder, CancellationToken ct = default); void Delete(string? relativeUrl); }`
  - `public sealed record ImageSaveResult(bool Success, string? RelativeUrl, string? ErrorMessage);`
  - `LocalImageStorage` DI'a singleton olarak kaydedilir; `IWebHostEnvironment` kullanır.
  - Göreli yol biçimi: `/uploads/<folder>/<guid><uzantı>`.

- [ ] **Adım 1: Başarısız olacak doğrulama testlerini yaz.**
```csharp
[Fact] public async Task Izinsiz_uzanti_reddedilir() { /* "x.gif" -> Success false, ErrorMessage bos degil */ }
[Fact] public async Task Iki_megabayt_ustu_reddedilir() { /* 2.1 MB icerik -> Success false */ }
[Fact] public async Task Uzantisi_png_ama_icerigi_gorsel_olmayan_dosya_reddedilir() { /* "x.png", govde "merhaba" -> Success false */ }
[Fact] public async Task Gecerli_dosya_kaydedilir_ve_goreli_yol_doner() { /* gercek PNG baytlari -> Success true, RelativeUrl "/uploads/" ile baslar, dosya diskte var */ }
```

- [ ] **Adım 2: Testleri çalıştırıp başarısız olduklarını gör.**
  Çalıştır: `dotnet test --filter ImageUploadTests`
  Beklenen: FAIL — `IImageStorage` yok.

- [ ] **Adım 3: `IImageStorage` ve `LocalImageStorage`'ı yaz.** Doğrulama sırası: uzantı (küçük harfe çevrilip karşılaştırılır), boyut, sihirli baytlar. Kabul edilen imzalar: JPEG `FF D8 FF`, PNG `89 50 4E 47`, WEBP `52 49 46 46` + 8. bayttan `57 45 42 50`. Dosya adı kullanıcıdan hiç alınmaz, `Guid.NewGuid()` + doğrulanmış uzantı ile yazılır. `Delete` yalnızca `/uploads/` ile başlayan yolları siler, aksi halde hiçbir şey yapmaz (harici URL'ler korunur).

- [ ] **Adım 4: `Program.cs`'te servisi kaydet** ve `wwwroot/uploads` klasörünü uygulama açılışında oluştur.

- [ ] **Adım 5: Review Focus #3 — büyük harfli uzantı testini ekle ve geçir.**
```csharp
[Fact] public async Task Buyuk_harfli_uzanti_kabul_edilir() { /* "x.JPG" + gecerli JPEG baytlari -> Success true */ }
```

- [ ] **Adım 6: Testleri çalıştırıp geçtiklerini gör.**
  Çalıştır: `dotnet test --filter ImageUploadTests`
  Beklenen: PASS

- [ ] **Adım 7: Review Focus #2 — büyük dosyanın anlaşılır hata vermesi.** Controller'larda dosya reddedilirse `ModelState.AddModelError(nameof(vm.ImageFile), result.ErrorMessage)` ile forma dönülecek. Kestrel'in ham 413'üne düşmemek için `Program.cs`'te `options.Limits.MaxRequestBodySize` 8 MB'a ayarlanacak (uygulama sınırı 2 MB'ın üstünde kalsın ki hata formda görünsün).
```csharp
[Fact] public async Task Buyuk_dosya_formda_hata_mesaji_gosterir() { /* 3 MB dosyayla POST -> 200 ve govdede hata metni */ }
```

- [ ] **Adım 8: Üç controller'ı ve altı view'ı güncelle.** Her birine `IFormFile? ImageFile` eklenir (`About`, `Banner`, `Project`), form `enctype="multipart/form-data"` alır, `ImageUrl` alanı formda kalır (harici URL girme imkânı korunur) ama artık zorunlu değil — dosya **veya** URL yeterli.

- [ ] **Adım 9: Review Focus #4 — dosya seçilmeden güncelleme testini ekle.**
```csharp
[Fact] public async Task Dosya_secilmeden_guncelleme_mevcut_gorseli_korur() { /* ImageFile null, ImageUrl degismemis -> kayittaki ImageUrl aynen kaliyor, dosya silinmiyor */ }
```

- [ ] **Adım 10: Eski dosyanın silinmesini uygula.** Güncellemede yeni dosya başarıyla kaydedildiyse, eski `ImageUrl` `/uploads/` ile başlıyorsa `Delete` çağrılır.

- [ ] **Adım 11: Tüm testleri çalıştır.**
  Çalıştır: `dotnet test`
  Beklenen: PASS

- [ ] **Adım 12: Panelden gerçek bir görsel yükleyip public tarafta göründüğünü doğrula.**

- [ ] **Adım 13: Commit.**
```bash
git add Portfolio Portfolio.Tests
git commit -m "feat: admin panelinden gorsel yukleme"
```

---

## Görev 8: İletişim formu, loglama ve temizlik

**Files:**
- Create: `Portfolio/Models/ContactFormVM.cs`
- Modify: `Portfolio/Controllers/DefaultController.cs`
- Modify: `Portfolio/Views/Shared/Components/_DefaultContactForm/Default.cshtml`
- Modify: `Portfolio/Views/AdminLayout/Layout.cshtml`
- Modify: `Portfolio/Program.cs`, `Portfolio/Portfolio.csproj`
- Delete: `Portfolio/Views/AdminLayout/HeadPartial.cshtml`, `AdminLayoutController.HeadPartial()`, `wwwroot/stitch_*`, `wwwroot/AdminTemplate/`
- Test: `Portfolio.Tests/ContactFormTests.cs`

**Interfaces:**
- Consumes: Görev 6'nın antiforgery ayarı.
- Produces: `ContactFormVM` — `Name`, `Email`, `MessageBody` (`UserMessage` ile aynı doğrulama nitelikleri) + `public string? Website { get; set; }` (honeypot, formda `aria-hidden` ve ekran dışı). Rate limiter politika adı `"contact"`.

- [ ] **Adım 1: Başarısız olacak testleri yaz.**
```csharp
[Fact] public async Task Gecerli_mesaj_kaydediliyor() { /* POST -> 302; UserMessages sayisi 1 artiyor */ }
[Fact] public async Task Gecersiz_eposta_kaydedilmiyor() { /* sayi artmiyor, 200 donuyor */ }
[Fact] public async Task Honeypot_dolu_ise_kaydedilmiyor() { /* Website="spam" -> sayi artmiyor, yine 302 donuyor */ }
[Fact] public async Task Dorduncu_mesaj_rate_limit_e_takiliyor() { /* ayni istemciden 4. POST -> 429 */ }
```

- [ ] **Adım 2: Testleri çalıştırıp başarısız olduklarını gör.**
  Çalıştır: `dotnet test --filter ContactFormTests`
  Beklenen: FAIL

- [ ] **Adım 3: `ContactFormVM`'i yaz, `DefaultController.CreateUserMessage`'ı ona çevir.** Honeypot doluysa hiçbir şey kaydetmeden `RedirectToAction("Index")` (spam gönderene başarı gibi görünür). Geçersiz model durumunda `View("Index")` yerine `RedirectToAction` + `TempData` hata mesajı kullanılacak — mevcut kod `View("Index")` döndürüyor ve bu, layout'suz anasayfayı bozuyor.

- [ ] **Adım 4: Rate limiter'ı ekle.** `AddRateLimiter` + `"contact"` adlı fixed-window politikası: 5 dakikada 3 istek, IP başına. POST action'a `[EnableRateLimiting("contact")]`.

- [ ] **Adım 5: Testleri çalıştırıp geçtiklerini gör.**
  Çalıştır: `dotnet test --filter ContactFormTests`
  Beklenen: PASS

- [ ] **Adım 6: Serilog'u ekle.**
```bash
dotnet add Portfolio package Serilog.AspNetCore
```
  `logs/portfolio-.log` yoluna günlük döngüyle yazacak, 14 gün tutacak.

- [ ] **Adım 7: Admin layout'taki JS hatasını düzelt.** `searchInput` null olabileceği için `if (searchInput) { ... }` kontrolü eklenecek.

- [ ] **Adım 8: Public klasördeki artıkları sil.** `wwwroot/stitch_portfolio.zip`, `wwwroot/stitch_modern_portfolio_admin_login.zip`, `wwwroot/stitch_portfolio/`, `wwwroot/stitch_modern_portfolio_admin_login/`, `wwwroot/AdminTemplate/`. Ayrıca ölü `Views/AdminLayout/HeadPartial.cshtml` ve `AdminLayoutController.HeadPartial()` action'ı.

- [ ] **Adım 9: Silinen dosyaların hiçbir yerden referans verilmediğini doğrula.**
  Çalıştır: `git grep -n "stitch_\|AdminTemplate\|HeadPartial"`
  Beklenen: çıktı boş.

- [ ] **Adım 10: Tüm testleri çalıştır.**
  Çalıştır: `dotnet test`
  Beklenen: PASS

- [ ] **Adım 11: Commit.**
```bash
git add -A Portfolio Portfolio.Tests
git commit -m "feat: iletisim formu korumasi, loglama ve public klasor temizligi"
```

---

## Görev 9: Tünel arkasında çalışma ayarları

**Files:**
- Modify: `Portfolio/Program.cs`
- Modify: `Portfolio/appsettings.json`
- Test: `Portfolio.Tests/ForwardedHeadersTests.cs`

**Interfaces:**
- Consumes: `LocalOnlyAttribute` (Görev 5), rate limiter (Görev 8).
- Produces: `UseForwardedHeaders` yapılandırması; `LocalOnly` ve rate limiter artık gerçek ziyaretçi IP'sini görür.

- [ ] **Adım 1: Başarısız olacak testi yaz.** Tünel arkasında gerçek ziyaretçi IP'si yalnızca `X-Forwarded-For`'dan okunabilir; bu okunmazsa her istek loopback görünür ve `LocalOnly` ile rate limiter işlevsiz kalır. Test tam bunu ölçüyor:
```csharp
[Fact]
public async Task Forwarded_For_ile_gelen_uzak_istek_admin_yolunda_404_alir()
{
    var client = _factory.CreateAnonymousClient();   // bağlantı loopback
    client.DefaultRequestHeaders.Add("X-Forwarded-For", "203.0.113.5");
    var response = await client.GetAsync("/Dashboard");
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}
```

- [ ] **Adım 2: Testi çalıştırıp başarısız olduğunu gör.**
  Çalıştır: `dotnet test --filter ForwardedHeadersTests`
  Beklenen: FAIL — header okunmadığı için bağlantı loopback sayılıyor, `LocalOnly` isteği geçiriyor ve 302 (giriş sayfası) dönüyor.

- [ ] **Adım 3: `Program.cs`'i güncelle.** `UseForwardedHeaders` (`XForwardedFor | XForwardedProto`, `KnownNetworks`/`KnownProxies` temizlenir çünkü proxy loopback'ten geliyor) **`UseAuthentication` öncesine** eklenir. `UseHttpsRedirection` yalnızca `app.Environment.IsDevelopment()` ise çağrılır — canlıda TLS'i tünel sonlandırıyor, yönlendirme açık kalırsa döngü oluşur.

- [ ] **Adım 4: `AllowedHosts`'u config'den okunur hale getir.** `appsettings.json`'da `"AllowedHosts": ""` kalacak; gerçek değer canlıda environment variable ile verilecek.

- [ ] **Adım 5: Kestrel'i localhost'a bağla.** `appsettings.json`'a `Kestrel:Endpoints:Http:Url` = `http://localhost:5080`.

- [ ] **Adım 6: Testleri çalıştırıp geçtiklerini gör.**
  Çalıştır: `dotnet test`
  Beklenen: PASS

- [ ] **Adım 7: Commit.**
```bash
git add Portfolio Portfolio.Tests
git commit -m "feat: tunel arkasinda calisma icin forwarded headers ayarlari"
```

---

## Görev 10: CI/CD hattı

**Files:**
- Create: `.github/workflows/ci.yml`

**Interfaces:**
- Consumes: Görev 2'nin test projesi.
- Produces: `master`'a push'ta build + test + publish + servis yeniden başlatma.

- [ ] **Adım 1: Workflow'u yaz.** `on: push: branches: [master]` — **`pull_request` tetikleyicisi eklenmeyecek** (self-hosted runner'da yabancı PR'ı kod çalıştırabilir). `runs-on: self-hosted`. Adımlar: checkout, `dotnet restore`, `dotnet build --no-restore -c Release`, `dotnet test --no-build -c Release`, `dotnet publish -c Release -o <yayın klasörü>`, servisi yeniden başlat.

- [ ] **Adım 2: Self-hosted runner'ı makineye kur.** Repo → Settings → Actions → Runners üzerinden; Windows servisi olarak çalışacak şekilde.

- [ ] **Adım 3: Repo ayarlarından dış katkıcıların workflow çalıştırmasını onaya bağla.** Settings → Actions → "Require approval for all external contributors".

- [ ] **Adım 4: Boş bir commit atıp hattın yeşil döndüğünü doğrula.**
  Çalıştır: `git commit --allow-empty -m "ci: hatti dogrula" && git push`
  Beklenen: Actions sekmesinde iş yeşil; test adımı entegrasyon testlerini de koşmuş.

- [ ] **Adım 5: Commit.**
```bash
git add .github/workflows/ci.yml
git commit -m "ci: build, test ve self-hosted runner ile deploy hatti"
```

---

## Görev 11: Public tarafın boş veriye dayanıklılığı ve meta bilgileri

**Files:**
- Modify: `Portfolio/ViewComponents/Default-*/*.cs` ve ilgili `Views/Shared/Components/*/Default.cshtml`
- Modify: `Portfolio/Views/Shared/Components/_DefaultHead/Default.cshtml`
- Test: `Portfolio.Tests/EmptyContentTests.cs`

**Interfaces:**
- Consumes: `Site:BaseUrl` (Görev 1).
- Produces: Her public bölüm, verisi yoksa kendini render etmez.

- [ ] **Adım 1: Review Focus #1 — başarısız olacak testi yaz.**
```csharp
[Fact] public async Task Tum_icerik_tablolari_bosken_anasayfa_200_doner()
{
    // tum icerik tablolarini temizle, sonra:
    var response = await client.GetAsync("/");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

- [ ] **Adım 2: Testi çalıştırıp başarısız olduğunu gör.**
  Çalıştır: `dotnet test --filter EmptyContentTests`
  Beklenen: FAIL — ilk kaydı varsayan view component'lerde null referans / `InvalidOperationException`.

- [ ] **Adım 3: View component'leri boş veriye dayanıklı hale getir.** `First()` yerine `FirstOrDefault()`; model null ise `Content(string.Empty)` döndür. Liste bekleyen bölümler boş listeyle sorunsuz render etmeli. Referans bölümü bu yüzden ayrıca ele alınacak (uydurma referanslar silinince boş kalacak).

- [ ] **Adım 4: Testi çalıştırıp geçtiğini gör.**
  Çalıştır: `dotnet test --filter EmptyContentTests`
  Beklenen: PASS

- [ ] **Adım 5: `_DefaultHead`'deki meta bilgilerini config'den üret.** `<title>`, `<meta name="description">`, Open Graph (`og:title`, `og:description`, `og:image`, `og:url`) ve canonical. Mutlak URL'ler `Site:BaseUrl` üzerinden kurulacak. **Gerçek metinler Görev 13'ün içerik kapısında kullanıcıdan gelecek**; bu adımda yalnızca config'den okuma altyapısı kurulur ve `"Alex Dev | Portfolio"` sabiti koddan çıkarılır.

- [ ] **Adım 6: Tüm testleri çalıştır.**
  Çalıştır: `dotnet test`
  Beklenen: PASS

- [ ] **Adım 7: Commit.**
```bash
git add Portfolio Portfolio.Tests
git commit -m "fix: public bolumleri bos veriye dayanikli hale getir, meta bilgilerini config'e tasi"
```

---

## Görev 12: Tailwind'i build'e geçir

**Files:**
- Create: `Portfolio/tailwind/public.config.js`, `admin.config.js`, `login.config.js`
- Create: `Portfolio/tailwind/input.css`
- Modify: `Portfolio/Views/Shared/Components/_DefaultHead/Default.cshtml`, `Portfolio/Views/Shared/Components/_AdminLayoutHead/Default.cshtml`, `Portfolio/Views/Auth/Login.cshtml`
- Modify: `Portfolio/Portfolio.csproj` (build hedefi)

**Interfaces:**
- Consumes: —
- Produces: `wwwroot/css/public.css`, `wwwroot/css/admin.css`, `wwwroot/css/login.css`.

- [ ] **Adım 1: Mevcut görünümün referans ekran görüntülerini al.** Anasayfanın her bölümü, admin panelinin bir index sayfası ve giriş sayfası. Karşılaştırma bunlara göre yapılacak.

- [ ] **Adım 2: Üç inline `tailwind.config` bloğunu üç ayrı dosyaya çıkar.** Token değerleri birebir kopyalanacak — public koyu (`primary: #c0c1ff`), admin açık (`primary: #3525cd`), login (`primary: #004ac6`). `darkMode: "class"` ayarı korunacak.

- [ ] **Adım 3: Üç CSS'i build et.**
```bash
npx tailwindcss -c Portfolio/tailwind/public.config.js -i Portfolio/tailwind/input.css -o Portfolio/wwwroot/css/public.css --minify
```
  (admin ve login için aynısı). `content` alanları ilgili view yollarını kapsayacak şekilde ayarlanacak, yoksa sınıflar budanır.

- [ ] **Adım 4: Üç view'daki `<script src="https://cdn.tailwindcss.com...">` ve inline config bloklarını `<link rel="stylesheet">` ile değiştir.**

- [ ] **Adım 5: Görsel karşılaştırma yap.** Adım 1'deki ekran görüntüleriyle birebir karşılaştır. Fark varsa `content` yollarını ve eksik sınıfları düzelt.

- [ ] **Adım 6: Build'i `.csproj`'a bağla.** Release build'inde üç CSS'i üreten bir `Target` eklenecek, böylece CI'da unutulamaz.

- [ ] **Adım 7: Tüm testleri çalıştır.**
  Çalıştır: `dotnet test`
  Beklenen: PASS

- [ ] **Adım 8: Commit.**
```bash
git add Portfolio
git commit -m "refactor: Tailwind'i CDN'den build cikisina gecir"
```

---

## Görev 13: Yedekleme, içerik kapısı ve yayın

**Files:**
- Create: `scripts/backup-db.ps1`
- Create: `README.md` (yayın ve kurtarma prosedürleri)

**Interfaces:**
- Consumes: Tüm önceki görevler.
- Produces: Yayında çalışan site.

- [ ] **Adım 1: Yedekleme scriptini yaz ve zamanla.** `scripts/backup-db.ps1`: `BACKUP DATABASE PortfolioDb` + `wwwroot/uploads` klasörünün kopyası, ikinci bir diske. Windows Task Scheduler ile günlük.

- [ ] **Adım 2: Yedeği bir kez geri yükleyerek doğrula.** Yedekten `PortfolioDb_RestoreTest` adıyla geri yükle, tablo sayılarını karşılaştır, sonra düşür. **Denenmemiş yedek yedek değildir.**

- [ ] **Adım 3: İÇERİK KAPISI — kullanıcıdan gelenler tamamlanmadan ilerlenmeyecek.**
  - [ ] Banner, 2 proje ve profil görselleri panelden yüklendi (Google Resimler'den alınan iki proje görseli değiştirildi).
  - [ ] 8 uydurma referans silindi.
  - [ ] Site başlığı, açıklama ve OG görseli config'e girildi.
  - [ ] `ContactInfos.Address` kararı uygulandı (karar yoksa alan yayınlanmıyor).
  - [ ] Alan adı rotası seçildi (ücretli domain + Cloudflare ya da ücretsiz Funnel adresi).

- [ ] **Adım 4: SQL Server yetkisini ver.** `NT AUTHORITY\SYSTEM` için `PortfolioDb` içinde kullanıcı oluştur ve `db_owner` ver. Yalnızca bu veritabanı; sunucu rolü verilmeyecek.

- [ ] **Adım 5: Yayın çıktısını al ve servisi kur.** `dotnet publish -c Release`; uygulamayı `LocalSystem` olarak çalışan bir Windows servisi yap.

- [ ] **Adım 6: Environment variable'ları makine düzeyinde tanımla.** `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__DefaultConnection`, `ADMIN_SEED_PASSWORD`, `AllowedHosts`, `Site__BaseUrl`.

- [ ] **Adım 7: Tüneli kur ve servis olarak çalıştır.** Yalnızca public yollar yayınlanacak; `/Auth/*` ve admin yolları yayınlanmayacak.

- [ ] **Adım 8: Yayını doğrula.**
  - Public adres dışarıdan (telefon, mobil veri) açılıyor ve HTTPS yeşil.
  - `/Dashboard` ve `/Auth/Login` dışarıdan **404** dönüyor.
  - Panel `localhost`'tan açılıyor, giriş yapılıyor, şifre değiştiriliyor.
  - Görseller yükleniyor ve public tarafta görünüyor.
  - İletişim formu çalışıyor, 4. mesaj rate limit'e takılıyor.
  - Makine yeniden başlatıldığında site kendiliğinden ayağa kalkıyor.

- [ ] **Adım 9: README'yi yaz.** Yayın adımları, environment variable listesi, şifre kurtarma prosedürü (`ADMIN_SEED_PASSWORD`'ü set et, `AspNetUsers` satırını sil, uygulamayı başlat), yedek geri yükleme, testlerin SQL Server Express gerektirdiği notu.

- [ ] **Adım 10: Commit.**
```bash
git add scripts README.md
git commit -m "docs: yayin, yedekleme ve kurtarma proseduerleri"
```
