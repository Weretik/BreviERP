using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Pricing.Sewing.Evaluation;
using Catalog.Application.Features.Product.Sewing;

namespace Catalog.Application.Features.Pricing.Sewing.ArticleSummary;

public sealed class GetSewingArticleSummaryQueryHandler(
    ISewingCompositionReadStore composition,
    ISewingPricingInputReader inputs,
    ISewingPricingSettingsReader settings,
    IPricingReferenceVersionReader references,
    ISewingPublishedPriceReader published,
    IProductReferenceReader referenceNames,
    TimeProvider timeProvider)
    : IQueryHandler<GetSewingArticleSummaryQuery, Result<SewingArticleSummary>>
{
    public async ValueTask<Result<SewingArticleSummary>> Handle(
        GetSewingArticleSummaryQuery query, CancellationToken cancellationToken)
    {
        if (query.ProductId <= 0)
            return Result.Invalid(new ValidationError("productId", "A positive Product ID is required."));
        var list = await composition.ReadArticlesAsync(query.ProductId, cancellationToken);
        if (list is null)
            return Result.NotFound();
        var shared = await composition.ReadCompositionAsync(query.ProductId, null, cancellationToken);
        if (shared is null)
            return Result.NotFound();
        var names = await referenceNames.GetSnapshotAsync(cancellationToken);

        var at = timeProvider.GetUtcNow();
        var publishedByArticle = list.Articles.Count == 0
            ? new Dictionary<long, SewingPublishedArticlePrice>()
            : (await published.ReadAsync([query.ProductId], cancellationToken))
                .SelectMany(product => product.Articles).ToDictionary(article => article.ArticleId);
        var evaluator = new SewingArticlePricingEvaluator(inputs, settings, references);
        var rows = new List<SewingArticleSummaryRow>(list.Articles.Count);
        foreach (var article in list.Articles)
        {
            var preview = await evaluator.EvaluateAsync(query.ProductId, article.Id, at, cancellationToken);
            if (!preview.IsSuccess)
                return Result.NotFound();
            var calculation = preview.Value.Calculation;
            publishedByArticle.TryGetValue(article.Id, out var current);
            var fabric = article.FabricId is { } fabricId ? names.Fabrics.GetValueOrDefault(fabricId) : null;
            rows.Add(new SewingArticleSummaryRow(article.Id, article.SaleSku, article.Active, article.SortOrder,
                article.FabricId, fabric?.Name, fabric?.Price, article.FabricQuantity,
                calculation.IsCalculated, calculation.Blockers,
                calculation.Breakdown,
                calculation.Prices,
                current is null ? null : new SewingArticlePublishedSummary(current.PublishedAt, current.Prices)));
        }

        var materials = shared.Materials.Where(line => line.ArticleId is null).Select(line =>
        {
            var reference = line.ReferenceId is not { } id ? null
                : line.Kind == "Fabric" ? names.Fabrics.GetValueOrDefault(id)
                : line.Kind == "Accessory" ? names.GarmentAccessories.GetValueOrDefault(id)
                : null;
            return new SewingCardMaterial(line.Id, line.Kind, line.ReferenceId, line.CostCode, reference?.Name,
                reference?.Price, line.Quantity, line.Unit, line.SortOrder, line.Comment);
        }).ToList();
        var operations = shared.Operations.Select(line =>
        {
            var reference = names.GarmentPartOperations.GetValueOrDefault(line.OperationId);
            return new SewingCardOperation(line.Id, line.OperationId, reference?.Name, reference?.Minutes,
                line.Quantity, line.ManualMinutes, line.SortOrder, line.Comment);
        }).ToList();

        return Result.Success(new SewingArticleSummary(list.ProductId, list.ModelCode, list.Version, at, rows,
            materials, operations));
    }
}
