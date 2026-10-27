using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Domain.Quotes.Variants;

namespace Crm.Application.Quotes.Pricing;

public sealed class QuotePriceBasisResolver(
    IQuotePriceBasisStore bases, IQuoteCatalogPriceReader catalog)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<QuotePricePreview>> ResolveAsync(
        QuotePriceIntent intent, Guid? basisToken, QuoteVariant? previous,
        Guid? quoteId, CrmActor actor, CancellationToken cancellationToken)
    {
        if (basisToken is not { } token)
        {
            if (previous is null)
                return Result.Conflict("basis_required");
            var saved = JsonSerializer.Deserialize<QuotePricePreview>(previous.PriceBasisJson, JsonOptions);
            return saved is not null && saved.Intent == intent
                ? Result.Success(saved)
                : Result.Conflict("basis_required_for_price_change");
        }

        var preview = await bases.FindAsync(token, quoteId, actor, cancellationToken);
        if (preview is null || preview.Intent != intent)
            return Result.Conflict("basis_missing_expired_or_changed");
        if (preview.Source is { SourceId: var sourceId } && sourceId != Guid.Empty)
        {
            var current = await catalog.ReadAsync(intent.Kind, intent.ProductId!.Value,
                intent.ArticleId, cancellationToken);
            if (current.SourceId != sourceId || current.RuleVersionId != preview.Source.RuleVersionId)
                return Result.Conflict("source_changed");
        }
        return Result.Success(preview);
    }
}
