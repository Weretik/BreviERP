using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Evaluation;
using Catalog.Domain.Pricing.Ppe;
using Catalog.Domain.Pricing.Sewing;
using Crm.Application.Quotes.Pricing;

namespace Host.Api.Crm;

public sealed class QuoteCatalogPriceReader(
    ISewingPublicationStore publications,
    ISewingCalculationStore calculations,
    ISewingPricingInputReader sewingInputs,
    IPpePricingInputReader ppeInputs) : IQuoteCatalogPriceReader
{
    public async Task<QuoteCatalogPrice> ReadAsync(
        string kind, int productId, long? articleId, CancellationToken cancellationToken)
    {
        if (productId <= 0)
            return Blocked(kind, productId, articleId, "InvalidProduct");

        if (kind == "sewing")
            return await ReadSewingAsync(productId, articleId, cancellationToken);
        if (kind == "ppe")
            return await ReadPpeAsync(productId, articleId, cancellationToken);
        return Blocked(kind, productId, articleId, "UnsupportedKind");
    }

    private async Task<QuoteCatalogPrice> ReadSewingAsync(
        int productId, long? articleId, CancellationToken cancellationToken)
    {
        if (articleId is not > 0)
            return Blocked("sewing", productId, articleId, "InvalidArticle");
        var input = await sewingInputs.ReadAsync(productId, articleId.Value, cancellationToken);
        if (input is null)
            return Blocked("sewing", productId, articleId, "MissingArticle");
        if (!input.IsActive)
            return Blocked("sewing", productId, articleId, "InactiveArticle");

        var publication = await publications.ReadCurrentAsync(productId, articleId.Value, cancellationToken);
        if (publication is null)
            return Blocked("sewing", productId, articleId, "MissingPublication");
        var saved = await calculations.FindAsync(productId, articleId.Value, publication.CalculationId, cancellationToken);
        if (saved?.Snapshot.Calculation is not { IsCalculated: true } calculation ||
            calculation.Prices is null ||
            saved.Snapshot.SettingsVersionId is null)
            return Blocked("sewing", productId, articleId, "InvalidPublicationBasis");
        if (saved.Snapshot.ProductId != productId || saved.Snapshot.ArticleId != articleId ||
            saved.Snapshot.CompositionVersion != input.CompositionVersion ||
            publication.Prices != calculation.Prices)
            return Blocked("sewing", productId, articleId, "StalePublicationBasis");

        return new("sewing", productId, articleId, publication.Id, publication.CalculationId, null,
            saved.Snapshot.CompositionVersion, saved.Snapshot.SettingsVersionId,
            saved.Snapshot.ReferenceVersionIds.ToArray(), SewingArticleCalculation.FormulaVersion,
            publication.Prices.Retail, publication.Prices.Wholesale, publication.Prices.LargeWholesale, []);
    }

    private async Task<QuoteCatalogPrice> ReadPpeAsync(
        int productId, long? articleId, CancellationToken cancellationToken)
    {
        if (articleId is not null)
            return Blocked("ppe", productId, articleId, "UnexpectedArticle");
        var inputs = await ppeInputs.ReadAsync([productId], cancellationToken);
        var input = inputs.SingleOrDefault(x => x.ProductId == productId);
        if (input is null)
            return Blocked("ppe", productId, null, "MissingProduct");
        var evaluated = PpeProductPricingEvaluator.Evaluate(input);
        if (!evaluated.Calculation.IsCalculated || evaluated.Calculation.Prices is null ||
            evaluated.Offer is null)
            return Blocked("ppe", productId, null,
                evaluated.Calculation.Blockers.Select(x => x.Code).ToArray());
        var values = evaluated.Offer.Values;
        var usesRule = !(values.ExternalRetail is > 0 && values.ExternalWholesale is > 0 &&
            evaluated.Offer.IsApproved);
        return new("ppe", productId, null, evaluated.Offer.Id, null,
            usesRule ? evaluated.Rule?.Id : null,
            null, null, [], PpeCalculator.FormulaVersion,
            evaluated.Calculation.Prices.Retail, evaluated.Calculation.Prices.Wholesale,
            evaluated.Calculation.Prices.LargeWholesale, []);
    }

    private static QuoteCatalogPrice Blocked(string kind, int productId, long? articleId, params string[] blockers)
        => new(kind, productId, articleId, Guid.Empty, null, null, null, null, [], "",
            null, null, null, blockers);
}
