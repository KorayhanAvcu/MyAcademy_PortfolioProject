using System.Net;

namespace Portfolio.Tests;

[Collection(PortfolioAppCollection.Name)]
public class PublicSiteTests
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
