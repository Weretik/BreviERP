using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Preview;
using Catalog.Application.Features.Pricing.Sewing.Evaluation;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Domain.Products.Entities;
using Catalog.Infrastructure.DataBase;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;

namespace SewingPricing.Jobs.Verification;

public sealed class SewingArticleDiagnosticReader(CatalogDbContext catalog, WorkbookSnapshot source,
    ISewingPricingInputReader inputReader, ISewingPricingSettingsReader settingsReader,
    IPricingReferenceVersionReader referenceReader)
{
    public async Task<SewingPricingDiagnostic> ReadAsync(string sku, DateTimeOffset at, CancellationToken cancellationToken)
    {
        var article = await catalog.Set<SewingArticle>().AsNoTracking().Where(x => x.SaleSku == sku)
            .Select(x => new { x.Id, x.ProductId }).SingleAsync(cancellationToken);
        var handler = new CalculateSewingArticleQueryHandler(inputReader, settingsReader, referenceReader);
        var strict = await handler.Handle(new(article.ProductId.Value, article.Id.Value, at), cancellationToken);
        if (!strict.IsSuccess)
            throw new InvalidDataException($"Article {sku} could not be resolved.");
        var snapshot = (await inputReader.ReadAsync(article.ProductId.Value, article.Id.Value, cancellationToken))!;
        var settings = await settingsReader.ReadCandidatesAsync(at, cancellationToken);
        var values = await referenceReader.ReadAsync(SewingPricingInputResolution.Keys(snapshot), at, cancellationToken);
        if (settings.Count == 0 || (settings.Count > 1 && settings[0].EffectiveFrom == settings[1].EffectiveFrom)
            || values.GroupBy(x => x.ReferenceKey).Any(group => group.Count() > 1))
            throw new InvalidDataException("Diagnostic calculation requires unambiguous effective settings and inputs.");
        var byKey = values.ToDictionary(x => x.ReferenceKey);
        // Review acknowledgement is in-memory only; real blockers remain attached and nothing is approved or published.
        var diagnostic = SewingArticleCalculator.Calculate(settings[0].Settings,
            SewingPricingInputResolution.Materials(snapshot, byKey).Select(x => x with { IsReviewed = true }).ToArray(),
            SewingPricingInputResolution.Operations(snapshot, byKey).Select(x => x with { IsReviewed = true }).ToArray());
        var row = source.Rows("Прайс на сайт").Single(row => row.Text(0) == sku);
        return new(sku, SewingArticleCalculation.FormulaVersion, strict.Value.SettingsVersionId,
            strict.Value.ReferenceVersionIds, diagnostic.Breakdown, diagnostic.Prices, strict.Value.Calculation.Blockers,
            SewingPricingParityComparer.Compare(row, diagnostic));
    }
}

