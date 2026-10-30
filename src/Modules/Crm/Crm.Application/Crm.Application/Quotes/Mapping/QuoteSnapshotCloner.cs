using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Pricing;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.Quotes.Mapping;

public static class QuoteSnapshotCloner
{
    public static IReadOnlyList<QuoteItem> Clone(QuoteVersion source)
        => source.Items.OrderBy(x => x.SortOrder).Select(item =>
            QuoteItem.Create(Guid.Empty, item.SortOrder, item.Title, item.PublicDescription,
                item.PublicComment, item.InternalNote, item.Included,
                item.Variants.OrderBy(x => x.SortOrder).Select(variant =>
                    QuoteVariant.Create(Guid.Empty, variant.SortOrder, variant.Title, variant.Kind,
                        variant.ProductId, variant.ArticleId, variant.Visible, variant.BasisToken,
                        variant.PriceBasisJson, variant.Tiers.Select(tier =>
                            QuoteTier.Create(tier.Code, tier.Amount, tier.Visible)), variant.CustomVersionId)))).ToArray();
}
