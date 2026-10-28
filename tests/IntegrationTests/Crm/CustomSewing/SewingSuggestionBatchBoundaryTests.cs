using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Suggestions;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Suggestions;
using Host.Api.Crm.CustomSewing.Suggestions;
using Microsoft.EntityFrameworkCore;
using Moq;
namespace IntegrationTests.Crm.CustomSewing;

public sealed class SewingSuggestionBatchBoundaryTests
{
    [Test]
    public async Task Truncated_composition_batch_and_excessive_distinct_keys_fail_closed_before_reference_resolution()
    {
        var identities = new Mock<ISewingSuggestionIdentityReader>();
        identities.Setup(x => x.ReadAsync("Зима", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SewingSuggestionIdentityBatch([new(1, 1, "M1", "Куртка A")], false));
        var compositions = new Mock<ISewingSuggestionCompositionReader>();
        compositions.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<SewingSuggestionIdentity>>(), "Зима", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SewingSuggestionCompositionBatch([], true));
        var references = new Mock<IPricingReferenceVersionReader>();
        var source = new SewingSuggestionSource(identities.Object, compositions.Object, references.Object);
        var truncated = await source.ReadAsync("Куртка", "winter", default);
        Assert.That(truncated.LimitReached, Is.True);
        Assert.That(truncated.Models, Is.Empty);
        compositions.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<SewingSuggestionIdentity>>(), "Зима", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SewingSuggestionCompositionBatch([new(1, 1, "SKU", 1, true,
                Enumerable.Range(1, 10001).Select(id => new SewingPricingMaterialRow(id, "Accessory:" + id, 1, "Piece", false, false, true, true)).ToArray(), [])], false));
        var excessive = await source.ReadAsync("Куртка", "winter", default);
        Assert.That(excessive.LimitReached, Is.True);
        Assert.That(excessive.Models, Is.Empty);
        references.Verify(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    [Test]
    public async Task Batch_composition_identity_limit_is_checked_before_database_access()
    {
        var counter = new SuggestionQueryCounter();
        await using var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=localhost;Database=must_not_connect;Username=test;Password=test").AddInterceptors(counter).Options);
        var result = await new SewingSuggestionCompositionReader(db).ReadAsync(
            Enumerable.Range(1, 2001).Select(id => new SewingSuggestionIdentity(id, id, "M" + id, "Куртка A")).ToArray(), "Зима", default);
        Assert.That(result.LimitReached, Is.True);
        Assert.That(result.Compositions, Is.Empty);
        Assert.That(counter.Reads, Is.Zero);
    }
}
