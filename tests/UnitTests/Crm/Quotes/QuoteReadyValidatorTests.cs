using System.Text.Json;
using Crm.Application.Quotes.PrepareQuote;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Pricing;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;

namespace UnitTests.Crm.Quotes;

public sealed class QuoteReadyValidatorTests
{
    [Test]
    public void Hidden_only_offer_and_missing_visible_price_block_ready()
    {
        Assert.That(QuoteReadyValidator.Validate(Version(false, 100, []))
            .Select(x => x.ErrorMessage), Does.Contain("no_visible_variants"));
        Assert.That(QuoteReadyValidator.Validate(Version(true, null, []))
            .Select(x => x.ErrorMessage), Does.Contain("invalid_visible_price_tier"));
        Assert.That(QuoteReadyValidator.Validate(Version(true, 0, []))
            .Select(x => x.ErrorMessage), Does.Contain("invalid_visible_price_tier"));
    }

    [Test]
    public void Saved_basis_with_blocker_cannot_be_issued_but_valid_historical_price_can()
    {
        Assert.That(QuoteReadyValidator.Validate(Version(true, 100, ["source_missing"]))
            .Select(x => x.ErrorMessage), Does.Contain("pricing_basis_blocked"));
        Assert.That(QuoteReadyValidator.Validate(Version(true, 100, [])), Is.Empty);
    }

    private static QuoteVersion Version(bool visible, decimal? retail, IReadOnlyList<string> blockers)
    {
        var basis = new QuotePricePreview(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-5),
            new("oneOffPpe", null, null, null, new(100, 90, 80, "Synthetic")), null,
            100, 90, 80, blockers, "UAH", "manual_no_extra_vat");
        var variant = QuoteVariant.Create(Guid.NewGuid(), 0, "Offer", "oneOffPpe", null, null,
            visible, basis.BasisToken,
            JsonSerializer.Serialize(basis, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            [QuoteTier.Create("retail", retail, true), QuoteTier.Create("wholesale", 90, false),
                QuoteTier.Create("largeWholesale", 80, false)]);
        return QuoteVersion.Create(new QuoteId(1), 1, null, "created", "Buyer", null, null, 7,
            [QuoteItem.Create(Guid.NewGuid(), 0, "Offer", null, null, null, true, [variant])]);
    }
}
