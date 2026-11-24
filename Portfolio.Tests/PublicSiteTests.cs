using System.Net;

namespace Portfolio.Tests;

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
