using Crm.Application.Quotes.BuildIntroduction;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.Introductions;

public static class IntroductionQuoteContentBuilder
{
    public static QuoteDraftContent Build(IntroductionPreparedDraft draft,
        IReadOnlyList<QuotePricePreview> previews)
    {
        var required = IntroductionSlot.Required(draft.Layout);
        if (draft.Slots.Count != required.Count || previews.Count != required.Count)
            throw new InvalidOperationException("introduction_slots_invalid");
        var items = new List<QuoteItemInput>(required.Count);
        for (var index = 0; index < required.Count; index++)
        {
            var slot = draft.Slots[index];
            var preview = previews[index];
            if (slot.Selection.Slot != required[index] || preview.Source is null ||
                preview.BasisToken == Guid.Empty || preview.Intent.Kind != "sewing" ||
                preview.Intent.ProductId != slot.Selection.ProductId ||
                preview.Intent.ArticleId != slot.Selection.ArticleId ||
                preview.LargeWholesale != slot.LargeWholesale ||
                preview.Source.SourceId != slot.CurrentPrice.SourceId ||
                preview.Blockers.Count > 0)
                throw new InvalidOperationException("introduction_price_or_selection_changed");
            items.Add(new QuoteItemInput(null, index, slot.ModelName,
                $"{slot.FabricName}; {slot.FabricComposition}; {slot.FabricDensity}",
                null, null, true,
                [new QuoteVariantInput(null, 0, slot.FabricName, true, preview.Intent,
                    preview.BasisToken, new QuoteTierVisibilityInput(false, false, true))]));
        }
        return new QuoteDraftContent(items);
    }
}
