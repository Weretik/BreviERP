using Catalog.Domain.Products;
using Catalog.Domain.Products;
using IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Catalog.Infrastructure.Persistence.Products;

namespace IntegrationTests.Catalog.Composition;

/// <summary>Fabric variants (CAT-015): a variant is added with its fabric in one transaction and removed with it.</summary>
[NonParallelizable]
public sealed class SewingVariantPersistenceTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task Variant_is_added_with_its_fabric_listed_and_removed_with_it()
    {
        int productId;
        FabricReferenceId fabricId;
        await using (var catalog = Catalog())
        {
            var product = await catalog.Products.SingleAsync(x => x.ModelCode == "MOD-025");
            productId = product.Id.Value;
            fabricId = (await catalog.Set<SewingMaterialLine>()
                .FirstAsync(x => x.ProductId == product.Id && x.Kind == SewingMaterialKind.Fabric)).FabricId!.Value;
        }

        await using (var catalog = Catalog())
        {
            var store = new SewingCompositionStore(catalog);
            var product = (await store.LoadAsync(productId, CancellationToken.None))!;
            var duplicate = SewingArticle.Create(default, product.Id, "246").Value;
            Assert.That((await store.AddArticleAsync(product,
                duplicate, fabricId, 2.5m, DateTimeOffset.UtcNow, CancellationToken.None)).IsFailure, Is.True);
        }

        await using (var catalog = Catalog())
        {
            var store = new SewingCompositionStore(catalog);
            var product = (await store.LoadAsync(productId, CancellationToken.None))!;
            Assert.That((await store.AddArticleAsync(product, SewingArticle.Create(default, product.Id, "999").Value,
                fabricId, 2.5m, DateTimeOffset.UtcNow, CancellationToken.None)).Value, Is.True);
        }

        long articleId;
        await using (var catalog = Catalog())
        {
            var list = (await new SewingCompositionReadStore(catalog).ReadArticlesAsync(productId, CancellationToken.None))!;
            var added = list.Articles.Single(x => x.SaleSku == "999");
            articleId = added.Id;
            Assert.Multiple(() =>
            {
                Assert.That(list.Articles.Count(x => x.SaleSku == "246"), Is.EqualTo(1));
                Assert.That((added.FabricId, added.FabricQuantity), Is.EqualTo(((int?)fabricId.Value, (decimal?)2.5m)));
            });
            Assert.That(await new SewingCompositionStore(catalog).HasPriceHistoryAsync(articleId, CancellationToken.None), Is.False);
        }

        await using (var catalog = Catalog())
        {
            var store = new SewingCompositionStore(catalog);
            var product = (await store.LoadAsync(productId, CancellationToken.None))!;
            product.RemoveArticle(ArticleId.Create(articleId), DateTimeOffset.UtcNow);
            Assert.That(await store.SaveAsync(CancellationToken.None), Is.True);
        }

        await using (var catalog = Catalog())
        {
            Assert.That(await catalog.Set<SewingArticle>().AnyAsync(x => x.SaleSku == "999"), Is.False);
            Assert.That(await catalog.Set<SewingMaterialLine>().AnyAsync(x => x.ArticleId == ArticleId.Create(articleId)), Is.False);
        }
    }
}
