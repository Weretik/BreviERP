using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Suggestions;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Suggestions;
using Crm.Application.CustomSewing.GetSuggestions;
using Host.Api.Crm.CustomSewing.Suggestions;
using Microsoft.EntityFrameworkCore;
using Moq;
namespace IntegrationTests.Crm.CustomSewing;

[NonParallelizable]
public sealed class SewingSuggestionBridgeTests : CrmDatabaseFixture
{
    [Test]
    public async Task Actual_catalog_batch_queries_reference_approval_and_unique_model_norms_do_not_leak_prices()
    {
        var counter = new SuggestionQueryCounter();
        await using var catalog = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(Connection).AddInterceptors(counter).Options);
        await catalog.Database.MigrateAsync();
        var now = DateTimeOffset.UtcNow;
        for (var id = 1; id <= 6; id++)
        {
            var key = ProductId.Create(id);
            var name = id == 6 ? "Куртка-пілот" : "Куртка модель " + id;
            var product = Product.Create(key, name, name, ProductSlug.Create("suggestion-model-" + id), ProductType.Sewing, now);
            product.SetModelCode("MODEL-" + id, now);
            var profile = ProductCatalogProfile.Create(key, true);
            profile.SetIdentity(null, id == 5 ? "Літо" : "Зима", "Piece", null);
            profile.SetSourceReview(id != 4, null);
            product.SetCatalogProfile(profile, now);
            product.AddArticle(SewingArticle.Create(default, key, "ARTICLE-" + id), now);
            if (id == 1) product.AddArticle(SewingArticle.Create(default, key, "ARTICLE-1-B"), now);
            catalog.Products.Add(product);
            await catalog.SaveChangesAsync(); // Persist generated article IDs before assigning article-scoped norms.
            var materials = product.Articles.Select((article, index) => SewingMaterialLine.Create(default, key, article.Id,
                SewingMaterialKind.Fabric, FabricReferenceId.Create(id * 10 + index), null, null, 2, SewingMaterialUnit.Metre, true, index,
                true, "Reviewer", now)).ToList();
            materials.Add(SewingMaterialLine.Create(default, key, null, SewingMaterialKind.Accessory, null,
                AccessoryReferenceId.Create(id == 3 ? 999 : 7), null, id == 2 ? 3 : 1, SewingMaterialUnit.Piece, true, 10,
                true, "Reviewer", now));
            materials.Add(SewingMaterialLine.Create(default, key, null, SewingMaterialKind.OtherCost, null, null,
                "IGNORED", 500, SewingMaterialUnit.Service, true, 11));
            product.ReplaceComposition(materials, [SewingOperationLine.Create(default, key, null, OperationReferenceId.Create(2),
                1, null, 0, true, true, "Reviewer", now)], now);
        }
        await catalog.SaveChangesAsync();
        catalog.ChangeTracker.Clear();
        var actual = new SewingSuggestionCompositionReader(catalog);
        var compositions = new Mock<ISewingSuggestionCompositionReader>();
        compositions.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<SewingSuggestionIdentity>>(), "Зима", It.IsAny<CancellationToken>()))
            .Returns((IReadOnlyCollection<SewingSuggestionIdentity> identities, string season, CancellationToken ct) => actual.ReadAsync(identities, season, ct));
        var approveOperation = true;
        var references = new Mock<IPricingReferenceVersionReader>();
        references.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Returns((IReadOnlyCollection<string> keys, DateTimeOffset _, CancellationToken _) => Task.FromResult<IReadOnlyList<PricingReferenceValue>>(
                keys.Where(x => x != "Accessory:999").Select(x => new PricingReferenceValue(x, 1, 987654.32m,
                    x.StartsWith("Fabric:", StringComparison.Ordinal) ? "Metre" : x.StartsWith("Operation:", StringComparison.Ordinal) ? "Minutes" : "Piece",
                    true, approveOperation || !x.StartsWith("Operation:", StringComparison.Ordinal), "PrivateSupplierSource")).ToArray()));
        var source = new SewingSuggestionSource(new SewingSuggestionIdentityReader(catalog), compositions.Object, references.Object);
        counter.Reads = 0;
        var result = SuggestionNormAggregator.Aggregate("Куртка", "winter", await source.ReadAsync("Куртка", "winter", default));
        Assert.That(counter.Reads, Is.EqualTo(4), "One identity + three bounded composition SQL queries, independent of article count.");
        compositions.Verify(x => x.ReadAsync(It.Is<IReadOnlyCollection<SewingSuggestionIdentity>>(ids => ids.Count == 4), "Зима",
            It.IsAny<CancellationToken>()), Times.Once);
        references.Verify(x => x.ReadAsync(It.Is<IReadOnlyCollection<string>>(keys => keys.Distinct().Count() == keys.Count),
            It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(result.SampleSize, Is.EqualTo(2));
        Assert.That(result.Norms.Single(x => x.Kind == "accessory").Quantity, Is.EqualTo(3));
        Assert.That(result.Norms.Single(x => x.Kind == "accessory").SampleSize, Is.EqualTo(2));
        Assert.That(result.Norms.Single(x => x.Kind == "operation").ReferenceId, Is.EqualTo(2));
        Assert.That(result.Norms.Single(x => x.Kind == "operation").Unit, Is.EqualTo("Operations"));
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.That(json, Does.Not.Contain("987654").And.Not.Contain("PrivateSupplierSource").And.Not.Contain("Price"));
        approveOperation = false;
        Assert.That((await source.ReadAsync("Куртка", "winter", default)).Models, Is.Empty);
    }
}
