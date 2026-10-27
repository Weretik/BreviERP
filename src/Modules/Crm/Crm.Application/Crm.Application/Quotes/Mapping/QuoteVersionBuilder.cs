using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Pricing;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.Quotes.Mapping;

public sealed class QuoteVersionBuilder(QuotePriceBasisResolver basisResolver)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<IReadOnlyList<QuoteItem>>> BuildAsync(
        QuoteDraftContent content, QuoteVersion? previous, Guid? quoteId,
        CrmActor actor, CancellationToken cancellationToken)
    {
        if (content.Items is null || content.Items.Count > 100 || content.Items.Any(x => x is null))
            return Result.Invalid(new ValidationError("items", "invalid_items"));
        var items = new List<QuoteItem>(content.Items.Count);
        foreach (var item in content.Items)
        {
            if (item.Variants is null || item.Variants.Count > 30 || item.Variants.Any(x => x is null))
                return Result.Invalid(new ValidationError("variants", "invalid_variants"));
            var previousItem = item.PublicId is { } itemId
                ? previous?.Items.SingleOrDefault(x => x.PublicId == itemId)
                : null;
            if (item.PublicId is not null && previousItem is null)
                return Result.Invalid(new ValidationError("itemId", "unknown_item"));
            var variants = new List<QuoteVariant>(item.Variants.Count);
            foreach (var variant in item.Variants)
            {
                if (variant.PriceIntent is null || variant.TierVisibility is null)
                    return Result.Invalid(new ValidationError("variant", "invalid_variant"));
                var previousVariant = variant.PublicId is { } variantId
                    ? previousItem?.Variants.SingleOrDefault(x => x.PublicId == variantId)
                    : null;
                if (variant.PublicId is not null && previousVariant is null)
                    return Result.Invalid(new ValidationError("variantId", "unknown_variant"));
                var resolved = await basisResolver.ResolveAsync(variant.PriceIntent, variant.BasisToken,
                    previousVariant, quoteId, actor, cancellationToken);
                if (!resolved.IsSuccess)
                    return Result.Conflict(resolved.Errors.ToArray());
                var preview = resolved.Value;
                var visibility = variant.TierVisibility;
                var tiers = new[]
                {
                    QuoteTier.Create("retail", preview.Retail, visibility.Retail),
                    QuoteTier.Create("wholesale", preview.Wholesale, visibility.Wholesale),
                    QuoteTier.Create("largeWholesale", preview.LargeWholesale, visibility.LargeWholesale)
                };
                variants.Add(QuoteVariant.Create(variant.PublicId ?? Guid.Empty, variant.SortOrder,
                    variant.Title, preview.Intent.Kind, preview.Intent.ProductId, preview.Intent.ArticleId,
                    variant.Visible, preview.BasisToken, JsonSerializer.Serialize(preview, JsonOptions), tiers));
            }
            items.Add(QuoteItem.Create(item.PublicId ?? Guid.Empty, item.SortOrder, item.Title,
                item.PublicDescription, item.PublicComment, item.InternalNote, item.Included, variants));
        }
        if (items.Select(x => x.PublicId).Distinct().Count() != items.Count ||
            items.SelectMany(x => x.Variants).Select(x => x.PublicId).Distinct().Count() !=
                items.Sum(x => x.Variants.Count))
            return Result.Invalid(new ValidationError("items", "duplicate_logical_id"));
        return Result.Success<IReadOnlyList<QuoteItem>>(items);
    }
}
