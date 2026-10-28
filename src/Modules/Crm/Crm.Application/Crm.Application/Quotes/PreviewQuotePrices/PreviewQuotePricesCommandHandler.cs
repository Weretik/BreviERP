using Ardalis.Result;
using Crm.Application.Quotes.Pricing;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.PreviewQuotePrices.Specifications;
using Mediator;

namespace Crm.Application.Quotes.PreviewQuotePrices;

public sealed class PreviewQuotePricesCommandHandler(
    IQuotePriceBasisStore store, IQuoteCatalogPriceReader catalog, IQuoteReadRepository quotes,
    IQuoteCustomVersionReader? customs = null)
    : ICommandHandler<PreviewQuotePricesCommand, Result<QuotePricePreview>>
{
    public async ValueTask<Result<QuotePricePreview>> Handle(
        PreviewQuotePricesCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId == Guid.Empty || command.Intent is null || command.Actor.UserId <= 0)
            return Result.Invalid(new ValidationError("requestId", "invalid_request"));
        if (command.QuoteId is { } quoteId)
        {
            var quote = await quotes.FirstOrDefaultAsync(
                new QuoteForPricePreviewSpec(quoteId, command.Actor), cancellationToken);
            if (quote is null) return Result.NotFound();
            if (quote.Archived)
                return Result.Conflict("quote_not_editable");
        }
        ResolvedCustomQuoteVersion? custom = null;
        if (command.Intent.Kind == "customSewing")
        {
            if (command.Intent.CustomVersionPublicId is not { } versionId || versionId == Guid.Empty || customs is null)
                return Result.Invalid(new ValidationError("intent", "invalid_custom_selection"));
            custom = await customs.ReadAsync(versionId, command.QuoteId, command.Actor, cancellationToken);
            if (custom is null) return Result.NotFound();
        }
        return await store.ExecuteAsync(command.RequestId, command.QuoteId, command.Actor, command.Intent,
            ct => CalculateAsync(command.Intent, custom, ct), cancellationToken);
    }

    private async Task<Result<QuotePricePreview>> CalculateAsync(QuotePriceIntent intent, ResolvedCustomQuoteVersion? custom, CancellationToken cancellationToken)
    {
        if (intent.Kind == "customSewing")
        {
            if (intent.CustomVersionPublicId is not { } versionId || versionId == Guid.Empty || intent.ProductId is not null
                || intent.ArticleId is not null || intent.Override is not null || intent.ManualPrices is not null || customs is null)
                return Result.Invalid(new ValidationError("intent", "invalid_custom_selection"));
            var saved = custom;
            if (saved is null) return Result.NotFound();
            var basis = saved.Snapshot.Basis;
            return Result.Success(new QuotePricePreview(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(15), intent,
                null, basis.Retail, basis.Wholesale, basis.LargeWholesale, basis.Blockers,
                "UAH", "custom_no_extra_vat", saved.Snapshot));
        }
        if (intent.CustomVersionPublicId is not null)
            return Result.Invalid(new ValidationError("intent", "unexpected_custom_version"));
        if (intent.Kind == "oneOffPpe")
        {
            if (intent.ProductId is not null || intent.ArticleId is not null || intent.Override is not null ||
                intent.ManualPrices is not { } prices || !ValidManual(prices.Retail) ||
                !ValidManual(prices.Wholesale) || !ValidManual(prices.LargeWholesale) ||
                string.IsNullOrWhiteSpace(prices.Reason) || prices.Reason.Length > 500)
                return Result.Invalid(new ValidationError("intent", "invalid_one_off_ppe"));
            return Result.Success(new QuotePricePreview(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(15),
                intent, null, prices.Retail, prices.Wholesale, prices.LargeWholesale, [],
                "UAH", "manual_no_extra_vat"));
        }

        if (intent.Kind is not ("sewing" or "ppe") || intent.ProductId is not > 0 ||
            intent.ManualPrices is not null ||
            (intent.Kind == "sewing" && (intent.ArticleId is not > 0 || intent.Override is not null)) ||
            (intent.Kind == "ppe" && intent.ArticleId is not null))
            return Result.Invalid(new ValidationError("intent", "invalid_catalog_selection"));

        var source = await catalog.ReadAsync(intent.Kind, intent.ProductId.Value, intent.ArticleId, cancellationToken);
        if (intent.Override is { } priceOverride &&
            (intent.Kind != "ppe" || !ValidManual(priceOverride.Amount) ||
             priceOverride.Tier is not ("retail" or "wholesale" or "largeWholesale") ||
             string.IsNullOrWhiteSpace(priceOverride.Reason) || priceOverride.Reason.Length > 500))
            return Result.Invalid(new ValidationError("override", "invalid_ppe_override"));
        if (source.Blockers.Count > 0)
            return Result.Success(new QuotePricePreview(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(15),
                intent, source, null, null, null, source.Blockers,
                "UAH", "catalog_no_extra_vat"));
        var retail = source.Retail;
        var wholesale = source.Wholesale;
        var large = source.LargeWholesale;
        if (intent.Override is { } change)
        {
            if (change.Tier == "retail") retail = change.Amount;
            else if (change.Tier == "wholesale") wholesale = change.Amount;
            else large = change.Amount;
        }
        return Result.Success(new QuotePricePreview(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(15),
            intent, source, retail, wholesale, large, [],
            "UAH", "catalog_no_extra_vat"));
    }

    private static bool ValidManual(decimal amount)
        => amount > 0 && amount < 1_000_000_000_000_000_000m &&
           decimal.Round(amount, 2) == amount;
}
