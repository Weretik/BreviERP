using System.Text.Json;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.Quotes.RepriceQuote.Mapping;

public static class QuoteRepriceSnapshot
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static QuoteDraftContent ToContent(QuoteVersion previous, IReadOnlyDictionary<Guid, Guid> tokens)
        => new(previous.Items.Select(item =>
            new QuoteItemInput(item.PublicId, item.SortOrder, item.Title, item.PublicDescription,
                item.PublicComment, item.InternalNote, item.Included,
                item.Variants.Select(variant =>
                    new QuoteVariantInput(variant.PublicId, variant.SortOrder, variant.Title, variant.Visible,
                        ReadBasis(variant).Intent, tokens[variant.PublicId], new QuoteTierVisibilityInput(
                            variant.Tiers.Single(x => x.Code == "retail").Visible,
                            variant.Tiers.Single(x => x.Code == "wholesale").Visible,
                            variant.Tiers.Single(x => x.Code == "largeWholesale").Visible))).ToArray())).ToArray());

    public static bool HasBlockedPrices(IEnumerable<QuoteItem> items)
        => items.SelectMany(x => x.Variants).Any(variant =>
            ReadBasis(variant).Blockers.Count > 0 || variant.Tiers.Any(tier => tier.Amount is null));

    private static QuotePricePreview ReadBasis(QuoteVariant variant)
        => JsonSerializer.Deserialize<QuotePricePreview>(variant.PriceBasisJson, JsonOptions)
            ?? throw new InvalidOperationException("Missing saved quote price basis.");
}
