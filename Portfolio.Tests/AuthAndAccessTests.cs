using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Data.Entities;
using Portfolio.Infrastructure;

namespace Portfolio.Tests;

[Collection(PortfolioAppCollection.Name)]
public class AuthAndAccessTests
{
    private readonly PortfolioAppFactory _factory;
    public AuthAndAccessTests(PortfolioAppFactory factory) => _factory = factory;

    private static async Task<HttpResponseMessage> PostLoginAsync(
        HttpClient client, string token, string email, string password) =>
        await client.PostAsync("/Auth/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token,
        }));

    [Fact]
    public async Task Dogru_eposta_ve_sifre_giris_yapar_ve_Dashboarda_yonlendirir()
    {
        var client = _factory.CreateAnonymousClient();
        var token = await PortfolioAppFactory.GetAntiforgeryTokenAsync(client, "/Auth/Login");

        var response = await PostLoginAsync(
            client, token, AdminSeeder.AdminEmail, PortfolioAppFactory.SeedPassword);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Dashboard", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Bes_yanlis_sifre_hesabi_kilitler_dogru_sifre_bile_reddedilir()
    {
        // Ortak admin kullanicisi kilitlenip diger testleri bozmasin diye ayri kullanici.
        const string email = "kilit@example.test";
        using (var scope = _factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var created = await users.CreateAsync(
                new AppUser { UserName = email, Email = email }, PortfolioAppFactory.SeedPassword);
            Assert.True(created.Succeeded, string.Join(", ", created.Errors.Select(e => e.Description)));
        }

        var client = _factory.CreateAnonymousClient();
        var token = await PortfolioAppFactory.GetAntiforgeryTokenAsync(client, "/Auth/Login");

        for (var i = 0; i < 5; i++)
            await PostLoginAsync(client, token, email, "Yanlis!Parola999");

        var response = await PostLoginAsync(client, token, email, PortfolioAppFactory.SeedPassword);

        // Basarili giris 302 doner; kilitli hesap formu 200 ile geri verir.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("kilitlendi", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Loopback_olmayan_istemci_Dashboard_icin_404_alir()
    {
        var client = _factory.CreateRemoteClient();
        var response = await client.GetAsync("/Dashboard");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Antiforgery_token_olmayan_POST_400_doner()
    {
        var client = _factory.CreateAnonymousClient();
        var response = await client.PostAsync("/Auth/Login", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["Email"] = AdminSeeder.AdminEmail,
                ["Password"] = PortfolioAppFactory.SeedPassword,
            }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
