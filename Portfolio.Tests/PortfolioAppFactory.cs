using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Portfolio.Data.Context;
using Portfolio.Infrastructure;

namespace Portfolio.Tests;

// Fabrika tek ortak test veritabanini olusturup siliyor; IClassFixture her test
// sinifina ayri bir ornek verdigi icin iki sinif paralel calisirken ayni
// veritabanini es zamanli olusturmaya calisiyordu. Collection fixture ile tek
// ornek paylasilir ve siniflar sirayla kosar.
[CollectionDefinition(Name)]
public class PortfolioAppCollection : ICollectionFixture<PortfolioAppFactory>
{
    public const string Name = "portfolio-app";
}

public class PortfolioAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Tek degisiklik noktasi: testlerin kullandigi veritabani adi. Canli "PortfolioDb" ASLA kullanilmaz.
    private const string TestDatabaseName = "PortfolioDb_Test";

    private const string TestConnectionString =
        $@"Server=KORAYHAN\SQLEXPRESS;Database={TestDatabaseName};Integrated Security=true;TrustServerCertificate=true";

    public const string SeedPassword = "Test!Parola123";

    // Uzak istemci testlerinde kullanilan, loopback OLMAYAN bagalanti adresi.
    private static readonly IPAddress RemoteAddress = IPAddress.Parse("203.0.113.5");

    // Ruling BB: tum test siniflari [Collection(PortfolioAppCollection.Name)] kullanmak
    // zorunda. Aliskanlikla IClassFixture<PortfolioAppFactory> yazan bir sinif ikinci bir
    // ornek olusturur, iki ornek ayni test veritabanini es zamanli olusturup siler ve hata
    // ("Database 'PortfolioDb_Test' already exists") nedeni hic soylemez. Bu sayac nedeni
    // acikca soyler.
    private static int _instanceCount;

    public PortfolioAppFactory()
    {
        if (Interlocked.Increment(ref _instanceCount) > 1)
            throw new InvalidOperationException(
                "PortfolioAppFactory'nin ikinci bir ornegi olusturuldu. Fabrika tek ortak test " +
                $"veritabanini ('{TestDatabaseName}') olusturup siliyor; iki ornek es zamanli " +
                "calisirsa birbirinin veritabanini dusurur. Test sinifi " +
                "IClassFixture<PortfolioAppFactory> yerine " +
                $"[Collection(PortfolioAppCollection.Name)] kullanmali.");

        // Devralinan CreateClient() de yonlendirme takip etmesin: yoksa bir test
        // 302/400/404 yerine yonlendirmenin hedefini (200) gorur.
        ClientOptions.BaseAddress = new Uri("https://localhost/");
        ClientOptions.AllowAutoRedirect = false;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Baglanti string'i builder zamaninda hazir olmali: Program.cs host build
        // sirasinda Migrate() + admin seed'i calistiriyor, yani asagidaki guard'dan
        // once. UseSetting host konfigurasyonuna yazar ve uygulama konfigurasyonuna
        // da akar; ConfigureAppConfiguration ikinci bir emniyet kemeri olarak kaliyor.
        builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionString);
        builder.UseSetting("ADMIN_SEED_PASSWORD", SeedPassword);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["ADMIN_SEED_PASSWORD"] = SeedPassword,
            });
        });

        // Guvenlik agi, host build SIRASINDA: cozulen baglanti test veritabanini
        // gostermiyorsa hicbir migrate/seed kodu calismadan dur.
        builder.ConfigureServices((context, _) =>
            EnsureTestDatabase(context.Configuration.GetConnectionString("DefaultConnection")));
    }

    public HttpClient CreateAnonymousClient() => CreateClient();

    /// <summary>
    /// Loopback olmayan bir bagalanti adresinden gelmis gibi gorunen istemci.
    /// Header degil, dogrudan <c>HttpContext.Connection.RemoteIpAddress</c> degistirilir;
    /// boylece test forwarded-headers ayarina bagimli olmaz.
    /// </summary>
    public HttpClient CreateRemoteClient() =>
        WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
                services.AddSingleton<IStartupFilter>(new RemoteIpStartupFilter(RemoteAddress))))
        .CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost/"),
            AllowAutoRedirect = false,
        });

    /// <summary>
    /// Verilen sayfanin HTML'inden antiforgery token'ini cikarir.
    /// Token'a bagli cerez istemcinin cerez kabinde kalir.
    /// </summary>
    public static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        var match = Regex.Match(
            html,
            """<input name="__RequestVerificationToken" type="hidden" value="([^"]+)" />""");

        if (!match.Success)
            throw new InvalidOperationException($"'{path}' sayfasinda antiforgery token bulunamadi.");

        return match.Groups[1].Value;
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        EnsureTestDatabase(db.Database.GetDbConnection().Database);
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        // Host kurulurken acilis kodu zaten migrate + seed yapiyor, ama yukaridaki
        // EnsureDeleted onu siliyor. Seed'i burada tekrar calistirmak testlerin
        // dayandigi onkosulu geri verir: test veritabaninda tek admin kullanicisi var
        // (e-posta AdminSeeder.AdminEmail, sifre SeedPassword).
        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        await seeder.RunAsync();
    }

    public new async Task DisposeAsync()
    {
        try
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            EnsureTestDatabase(db.Database.GetDbConnection().Database);
            await db.Database.EnsureDeletedAsync();
        }
        finally
        {
            // Guard ya da drop firlatirsa host sizmasin.
            await base.DisposeAsync();
        }
    }

    // Guvenlik agi: baglanti test veritabanini gostermiyorsa silme/migrate yapmadan dur.
    private static void EnsureTestDatabase(string? connectionStringOrDatabaseName)
    {
        if (string.IsNullOrWhiteSpace(connectionStringOrDatabaseName))
            throw new InvalidOperationException(
                "Test fabrikasi icin baglanti bilgisi cozulemedi; " +
                $"yalnizca '{TestDatabaseName}' veritabani kabul edilir.");

        var name = connectionStringOrDatabaseName.Contains('=')
            ? new SqlConnectionStringBuilder(connectionStringOrDatabaseName).InitialCatalog
            : connectionStringOrDatabaseName;

        if (name != TestDatabaseName)
            throw new InvalidOperationException(
                $"Test fabrikasi '{name}' veritabanina yonlendi; yalnizca '{TestDatabaseName}' kabul edilir.");
    }

    private sealed class RemoteIpStartupFilter : IStartupFilter
    {
        private readonly IPAddress _address;
        public RemoteIpStartupFilter(IPAddress address) => _address = address;

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                app.Use(async (context, nextMiddleware) =>
                {
                    context.Connection.RemoteIpAddress = _address;
                    await nextMiddleware();
                });
                next(app);
            };
    }
}
