using Catalog.Application.Features.Product.Sewing;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

public sealed class SewingCompositionQueryTests
{
    [TestCase(null)]
    [TestCase(246L)]
    public void Composition_projection_translates_for_shared_and_article_scopes(long? articleId)
    {
        using var db = CreateContext();
        var query = new SewingCompositionReadStore(db).CompositionQuery(101, articleId);
        Assert.That(query.ToQueryString(), Does.Contain("catalog.\"Products\""));
    }

    [Test]
    public void Article_projection_translates_without_loading_product_entities()
    {
        using var db = CreateContext();
        var query = new SewingCompositionReadStore(db).ArticlesQuery(101);
        Assert.That(query.ToQueryString(), Does.Contain("catalog.\"Products\""));
    }

    private static CatalogDbContext CreateContext() => new(new DbContextOptionsBuilder<CatalogDbContext>()
        .UseNpgsql("Host=localhost;Database=sql_translation_only;Username=test;Password=test").Options);
}
