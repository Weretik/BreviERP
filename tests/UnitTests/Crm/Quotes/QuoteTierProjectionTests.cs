using System.Text.Json;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Pricing;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;

namespace UnitTests.Crm.Quotes;

public sealed class QuoteTierProjectionTests
{
    [Test]
    public void Historical_tiers_have_semantic_order_independent_of_database_row_order()
    {
        var basis = new QuotePricePreview(Guid.NewGuid(), DateTimeOffset.UtcNow,
            new("oneOffPpe", null, null, null, new(100, 90, 80, "Synthetic")), null,
            100, 90, 80, [], "UAH", "manual_no_extra_vat");
        var variant = QuoteVariant.Create(Guid.NewGuid(), 0, "Synthetic", "oneOffPpe", null, null, true,
            basis.BasisToken, JsonSerializer.Serialize(basis, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            [QuoteTier.Create("largeWholesale", 80, false), QuoteTier.Create("retail", 100, true),
                QuoteTier.Create("wholesale", 90, true)]);
        var version = QuoteVersion.Create(new QuoteId(1), 1, null, "created", "Synthetic", null, null, 17,
            [QuoteItem.Create(Guid.NewGuid(), 0, "Synthetic", "", "", "", true, [variant])]);
        var tiers = QuoteDetailMapper.MapVersion(version).Items.Single().Variants.Single().Tiers;
        Assert.That(tiers.Select(x => x.Code), Is.EqualTo(new[] { "retail", "wholesale", "largeWholesale" }));
        Assert.That(tiers.Select(x => x.Amount), Is.EqualTo(new decimal?[] { 100, 90, 80 }));
        Assert.That(tiers.Last().Visible, Is.False);
    }
}
