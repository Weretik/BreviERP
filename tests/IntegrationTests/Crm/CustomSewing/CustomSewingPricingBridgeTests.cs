using Catalog.Application.Contracts.Pricing;
using Crm.Domain.CustomSewing.Composition;
using Host.Api.Crm.CustomSewing.Pricing;
using Moq;

namespace IntegrationTests.Crm.CustomSewing;

public sealed class CustomSewingPricingBridgeTests
{
    private static CustomSewingPricing Pricing(decimal fabricCost = 100)
    {
        var at = DateTimeOffset.UtcNow.AddDays(-1);
        var settings = new Mock<ISewingPricingSettingsReader>();
        settings.Setup(x => x.ReadCandidatesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SewingPricingSettingsSelection(10, 1, at, "sewing-sheet-v1",
                new(1000, 10, 100, 10, .25m, .1m, .1m, .1m, .1m, 10000, 2, 1.5m, 1.25m))]);
        var references = new Mock<IPricingReferenceVersionReader>();
        references.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PricingReferenceValue("Fabric:1", 101, fabricCost, "Meters", true, true, "synthetic"),
                new PricingReferenceValue("Operation:2", 102, 10, "Minutes", true, true, "synthetic")]);
        return new(settings.Object, references.Object);
    }

    [Test]
    public async Task Adapter_preserves_golden_prices_and_server_provenance()
    {
        var input = CustomSewingPersistenceTests.Configuration with
        {
            Delivery = 5,
            Packaging = 5,
            Applications = [new("embroidery", "Chest", null, 12, 30, true)]
        };
        var at = DateTimeOffset.UtcNow;
        var result = await Pricing().CalculateAsync(input, at, default);
        Assert.That(result.Retail, Is.EqualTo(310));
        Assert.That(result.Wholesale, Is.EqualTo(240));
        Assert.That(result.LargeWholesale, Is.EqualTo(205));
        Assert.That(result.InternalCost, Is.EqualTo(152));
        Assert.That(result.ExpiresAt, Is.EqualTo(at.AddMinutes(15)));
        Assert.That(result.References.Select(x => x.VersionId), Is.EquivalentTo(new long[] { 101, 102 }));
    }

    [Test]
    public async Task Unapproved_sale_and_unverified_artwork_block_a_priced_version()
    {
        var input = CustomSewingPersistenceTests.Configuration with
        {
            Applications = [new("print", "Chest", Guid.NewGuid(), 12, 30, false)]
        };
        var result = await Pricing().CalculateAsync(input, DateTimeOffset.UtcNow, default);
        Assert.That(result.Retail, Is.Null);
        Assert.That(result.Blockers, Does.Contain("ApplicationSaleNotApproved").And.Contain("ArtworkFileNotVerified"));
    }

    [Test]
    public async Task A_decimal_price_that_cannot_fit_quote_storage_is_a_blocker_before_version_save()
    {
        var input = CustomSewingPersistenceTests.Configuration with { Fabrics = [new(1, 100000, "Meters")] };
        var result = await Pricing(9999999999999m).CalculateAsync(input, DateTimeOffset.UtcNow, default);
        Assert.That(result.Retail, Is.Null);
        Assert.That(result.Blockers, Does.Contain("PriceOutOfRange"));
    }
    [Test]
    public async Task A_large_valid_price_within_numeric_20_2_is_not_arbitrarily_blocked()
    {
        var input = CustomSewingPersistenceTests.Configuration with { Fabrics = [new(1, 100000, "Meters")] };
        var result = await Pricing(100000000000m).CalculateAsync(input, DateTimeOffset.UtcNow, default);
        Assert.That(result.IsCalculated, Is.True);
        Assert.That(result.Retail, Is.GreaterThan(10000000000000000m));
        Assert.That(result.Blockers, Is.Empty);
    }

}
