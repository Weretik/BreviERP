using Crm.Domain.Quotes.Introductions;
using Crm.Domain.Quotes.Items;

namespace Crm.Application.Quotes.CreateQuote;

/// <summary>Guards against a second catalog read changing price or selection after the introduction snapshot was frozen.</summary>
public static class IntroductionQuotePinValidator
{
    public static bool Matches(IntroductionPublicSnapshot snapshot, IReadOnlyList<QuoteItem> items)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(items);
        if (snapshot.Slots is null || snapshot.MappingVersionId == Guid.Empty ||
            snapshot.EmailTemplateVersionId == Guid.Empty) return false;
        IReadOnlyList<IntroductionSlot> slots;
        try { slots = IntroductionSlot.Required(snapshot.Layout); }
        catch (IntroductionValidationException) { return false; }
        if (snapshot.Slots.Count != slots.Count || items.Count != slots.Count ||
            snapshot.Slots.Select(x => x.Slot).Distinct().Count() != slots.Count)
            return false;
        var ordered = items.OrderBy(x => x.SortOrder).ToArray();
        for (var index = 0; index < slots.Count; index++)
        {
            var pin = snapshot.Slots.SingleOrDefault(x => x.Slot == slots[index]);
            var item = ordered[index];
            if (pin is null || pin.ProductId <= 0 || pin.ArticleId <= 0 ||
                pin.LargeWholesale <= 0 || item.SortOrder != index || !item.Included ||
                item.Variants.Count != 1) return false;
            var variant = item.Variants.Single();
            if (!variant.Visible || variant.Kind != "sewing" ||
                variant.ProductId != pin.ProductId || variant.ArticleId != pin.ArticleId ||
                variant.Tiers.Count != 3) return false;
            var large = variant.Tiers.SingleOrDefault(x => x.Code == "largeWholesale");
            if (large is not { Visible: true, Currency: "UAH" } ||
                large.Amount != pin.LargeWholesale ||
                variant.Tiers.Any(x => x.Code != "largeWholesale" && x.Visible))
                return false;
        }
        return true;
    }
}
