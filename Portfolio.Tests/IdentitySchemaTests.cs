using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Data.Context;

namespace Portfolio.Tests;

[Collection(PortfolioAppCollection.Name)]
public class IdentitySchemaTests
{
    private readonly PortfolioAppFactory _factory;
    public IdentitySchemaTests(PortfolioAppFactory factory) => _factory = factory;

    [Fact]
    public async Task Identity_tablolari_olusuyor_icerik_tablolari_korunuyor()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Identity tablosu gercekten sorgulanabiliyor (tablo yoksa SqlException atar).
        // Sayiya DEGIL semaya bakiliyor: Task 4 acilista seed eklediginde
        // bu test kirilmasin.
        Assert.False(await db.Users.Where(u => u.Email == "yok@example.com").AnyAsync());

        // 13 icerik DbSet'i hala modelde
        var names = db.Model.GetEntityTypes().Select(t => t.ClrType.Name).ToList();
        foreach (var n in new[] { "About", "Banner", "Category", "ContactInfo", "Education",
                                  "Experience", "Project", "Service", "Skill", "TechStack",
                                  "Testimonial", "ProjectTechStack", "UserMessage" })
            Assert.Contains(n, names);

        // NOT: "Admin entity'si kalkmis" iddiasi Task 4'e tasindi (Ruling U).
    }
}
