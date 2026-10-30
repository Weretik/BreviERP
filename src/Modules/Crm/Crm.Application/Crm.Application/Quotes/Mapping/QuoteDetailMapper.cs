using System.Text.Json;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.Quotes.Mapping;

public static class QuoteDetailMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static QuoteDetail Map(
        Quote quote, QuoteVersion version, bool canEdit)
        => new(quote.PublicId, version.CustomerPublicId, quote.ContactPublicId, quote.RecipientName,
            quote.Revision, quote.State, quote.Archived, quote.Number, quote.OwnerId.Value,
            quote.CreatedAt, quote.UpdatedAt, canEdit,
            MapVersion(version), quote.SourceQuotePublicId, quote.SourceVersionPublicId);

    public static QuoteVersionView MapVersion(QuoteVersion version)
        => new QuoteVersionView(version.PublicId, version.VersionNumber, version.Action,
                version.RecipientName, version.CustomerPublicId, version.ContactPublicId,
                version.Currency, version.TaxBasis, version.ActorId, version.CreatedAt,
                version.Items.OrderBy(x => x.SortOrder).Select(item =>
                    new QuoteItemView(item.PublicId, item.SortOrder, item.Title,
                        item.PublicDescription, item.PublicComment, item.InternalNote, item.Included,
                        item.Variants.OrderBy(x => x.SortOrder).Select(variant =>
                            new QuoteVariantView(variant.PublicId, variant.SortOrder, variant.Title,
                                variant.Kind, variant.ProductId, variant.ArticleId, variant.Visible,
                                JsonSerializer.Deserialize<QuotePricePreview>(variant.PriceBasisJson, JsonOptions)!,
                                variant.Tiers.OrderBy(tier => tier.Code == "retail" ? 0 : tier.Code == "wholesale" ? 1 : 2)
                                .Select(tier => new QuoteTierView(
                                    tier.Code, tier.Amount, tier.Visible, tier.Currency)).ToArray())).ToArray()))
                    .ToArray());
}
