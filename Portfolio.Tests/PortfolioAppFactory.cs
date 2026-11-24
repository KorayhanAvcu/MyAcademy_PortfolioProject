using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Data.Context;

namespace Portfolio.Tests;

public class PortfolioAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Tek degisiklik noktasi: testlerin kullandigi veritabani adi. Canli "PortfolioDb" ASLA kullanilmaz.
    private const string TestDatabaseName = "PortfolioDb_Test";

    private const string TestConnectionString =
        $@"Server=KORAYHAN\SQLEXPRESS;Database={TestDatabaseName};Integrated Security=true;TrustServerCertificate=true";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["ADMIN_SEED_PASSWORD"] = "Test!Parola123",
            });
        });
    }

    public HttpClient CreateAnonymousClient() =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost/"),
            AllowAutoRedirect = false,
        });

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        EnsureTestDatabase(db);
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            EnsureTestDatabase(db);
            await db.Database.EnsureDeletedAsync();
        }
        await base.DisposeAsync();
    }

    // Guvenlik agi: baglanti test veritabanini gostermiyorsa silme/migrate yapmadan dur.
    private static void EnsureTestDatabase(AppDbContext db)
    {
        var name = db.Database.GetDbConnection().Database;
        if (name != TestDatabaseName)
            throw new InvalidOperationException(
                $"Test fabrikasi '{name}' veritabanina yonlendi; yalnizca '{TestDatabaseName}' kabul edilir.");
    }
}
