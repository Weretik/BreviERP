using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Domain.Pricing.Ppe;
using Catalog.Domain.Pricing.Sewing;
using Host.Api.Crm;
using Moq;

namespace IntegrationTests.Crm.Quotes;

public sealed class QuoteCatalogPriceReaderTests
{
    private readonly Mock<ISewingPublicationStore> _publications = new();
    private readonly Mock<ISewingCalculationStore> _calculations = new();
    private readonly Mock<ISewingPricingInputReader> _sewingInputs = new();
    private readonly Mock<IPpePricingInputReader> _ppeInputs = new();

    private QuoteCatalogPriceReader Reader => new(
        _publications.Object, _calculations.Object, _sewingInputs.Object, _ppeInputs.Object);

    [Test]
    public async Task ApprovedExternalPpePricesUseOfferWithoutRule()
    {
        var offerId = Guid.NewGuid();
        var offer = new PpeOfferVersion(offerId, 42,
            new PpeOfferValues(null, null, 120m, 80m, "fixture", "2026-10-26", true),
            true, "tester", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        _ppeInputs.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PpePricingProductInput(42, true, offer, null)]);

        var price = await Reader.ReadAsync("ppe", 42, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(price.Blockers, Is.Empty);
            Assert.That(price.SourceId, Is.EqualTo(offerId));
            Assert.That(price.RuleVersionId, Is.Null);
            Assert.That(price.Retail, Is.EqualTo(120m));
            Assert.That(price.Wholesale, Is.EqualTo(100m));
            Assert.That(price.LargeWholesale, Is.EqualTo(80m));
        });
    }

    [Test]
    public async Task PpeFallbackPinsApprovedRuleAndBlocksInactiveProduct()
    {
        var offerId = Guid.NewGuid();
        var ruleId = Guid.NewGuid();
        var offer = new PpeOfferVersion(offerId, 42,
            new PpeOfferValues("base", 100m, null, null, "fixture", "2026-10-26", true),
            true, "tester", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        var rule = new PpeRuleVersion(ruleId,
            new PpeRuleValues("base", new PpeMarkups(.2m, .1m, .05m), "fixture", "2026-10-26"),
            true, "tester", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        _ppeInputs.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PpePricingProductInput(42, true, offer, rule)]);

        var price = await Reader.ReadAsync("ppe", 42, null, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(price.Blockers, Is.Empty);
            Assert.That(price.SourceId, Is.EqualTo(offerId));
            Assert.That(price.RuleVersionId, Is.EqualTo(ruleId));
            Assert.That(price.Retail, Is.EqualTo(120m));
            Assert.That(price.Wholesale, Is.EqualTo(110m));
            Assert.That(price.LargeWholesale, Is.EqualTo(105m));
        });

        _ppeInputs.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PpePricingProductInput(42, false, offer, rule)]);
        var blocked = await Reader.ReadAsync("ppe", 42, null, CancellationToken.None);
        Assert.That(blocked.Blockers, Does.Contain("InactiveProduct"));
    }

    [Test]
    public async Task SewingRequiresCurrentPublication()
    {
        _sewingInputs.Setup(x => x.ReadAsync(52, 53, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SewingArticlePricingSnapshot(52, 53, "SKU", 4, true, [], []));

        var price = await Reader.ReadAsync("sewing", 52, 53, CancellationToken.None);

        Assert.That(price.Blockers, Does.Contain("MissingPublication"));
        Assert.That(price.Retail, Is.Null);
        _calculations.VerifyNoOtherCalls();
    }
}
