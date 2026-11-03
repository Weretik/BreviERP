using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Evaluation;
using Catalog.Application.Features.Product.Overview.Prices.Specifications;
using Catalog.Domain.Products.Enums;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Pricing.Sewing.Recalculation;

public sealed class RecalculateSewingPricesCommandHandler(
    ICatalogReadRepository<ProductEntity> products,
    SewingPriceStatusReader status,
    ISewingPricingTransaction transactions,
    ISewingCalculationStore calculations,
    ISewingPublicationStore publications,
    ISewingPricingInputReader inputs,
    ISewingPricingSettingsReader settings,
    IPricingReferenceVersionReader references,
    TimeProvider timeProvider)
    : ICommandHandler<RecalculateSewingPricesCommand, Result<SewingRecalculationReport>>
{
    public async ValueTask<Result<SewingRecalculationReport>> Handle(
        RecalculateSewingPricesCommand command, CancellationToken cancellationToken)
    {
        var sewingIds = await products.ListAsync(new ProductIdsByTypeSpec(ProductType.Sewing), cancellationToken);
        var ids = command.ProductIds is null ? sewingIds : sewingIds.Intersect(command.ProductIds).ToList();
        if (command.ProductIds is not null && ids.Count != command.ProductIds.Distinct().Count())
            return Result.NotFound();

        var at = timeProvider.GetUtcNow();
        var models = await status.ReadAsync(ids, at, cancellationToken);
        var changes = new List<SewingRecalculationChange>();
        var blocked = new List<SewingRecalculationBlocked>();
        var unchanged = 0;
        foreach (var model in models)
        foreach (var article in model.Articles.Where(x => x.Active))
        {
            if (article.NeedsPublication)
                changes.Add(new(model.ProductId, model.ModelCode, article.ArticleId, article.SaleSku,
                    article.Published?.Prices, article.Live.Prices!));
            else if (!article.Live.IsCalculated)
                blocked.Add(new(model.ProductId, model.ModelCode, article.ArticleId, article.SaleSku,
                    article.Live.Blockers.Select(x => x.Code).Distinct().ToList()));
            else
                unchanged++;
        }

        if (command.DryRun || changes.Count == 0)
            return Result.Success(new SewingRecalculationReport(false, unchanged, changes, blocked));

        var evaluator = new SewingArticlePricingEvaluator(inputs, settings, references);
        foreach (var product in changes.GroupBy(x => x.ProductId))
        {
            // One transaction per model, like a single «Опублікувати»: a model is published whole or not at all.
            await using var transaction = await transactions.BeginAsync(product.Key, cancellationToken);
            foreach (var change in product)
            {
                var preview = await evaluator.EvaluateAsync(change.ProductId, change.ArticleId, at, cancellationToken);
                if (!preview.IsSuccess || !preview.Value.Calculation.IsCalculated)
                    return Result.Conflict($"Inputs of {change.ModelCode ?? change.ProductId.ToString()} changed during recalculation; nothing after it was saved.");
                var saved = new SavedSewingCalculation(Guid.NewGuid(), Guid.NewGuid(), at, command.Actor, preview.Value);
                await calculations.SaveAsync(saved, cancellationToken);
                await publications.PublishAsync(new PublishedSewingPrice(Guid.NewGuid(), saved.Id, change.ProductId,
                    change.ArticleId, at, command.Actor, preview.Value.Calculation.Prices!), cancellationToken);
            }

            await transactions.CommitAsync(cancellationToken);
        }

        return Result.Success(new SewingRecalculationReport(true, unchanged, changes, blocked));
    }
}
