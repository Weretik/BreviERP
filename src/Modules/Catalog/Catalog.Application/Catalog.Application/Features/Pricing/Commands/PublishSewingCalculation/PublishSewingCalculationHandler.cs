using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Shared.Sewing.Evaluation;
using Catalog.Application.Abstractions.Pricing.Calculations;
using Catalog.Application.Abstractions.Pricing.Inputs;
using Catalog.Application.Abstractions.Pricing.Publications;
using Catalog.Application.Abstractions.Pricing.Settings;

namespace Catalog.Application.Features.Pricing.Commands.PublishSewingCalculation;

public sealed class PublishSewingCalculationHandler(
    ISewingPricingTransaction transactions, ISewingCalculationStore calculations,
    ISewingPublicationStore publications, ISewingPricingInputReader inputs,
    ISewingPricingSettingsReader settings, IPricingReferenceVersionReader references)
    : ICommandHandler<PublishSewingCalculationCommand, Result<PublishedSewingPrice>>
{
    public async ValueTask<Result<PublishedSewingPrice>> Handle(
        PublishSewingCalculationCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await transactions.BeginAsync(command.ProductId, cancellationToken);
        var saved = await calculations.FindAsync(command.ProductId, command.ArticleId, command.CalculationId, cancellationToken);
        if (saved is null)
            return Result.NotFound();
        var previous = await publications.FindPublicationAsync(saved.Id, cancellationToken);
        if (previous is not null)
            return Result.Success(previous);
        var at = DateTimeOffset.UtcNow;
        var current = await new SewingArticlePricingEvaluator(inputs, settings, references)
            .EvaluateAsync(command.ProductId, command.ArticleId, at, cancellationToken);
        if (!saved.Snapshot.Calculation.IsCalculated || !current.IsSuccess || !current.Value.Calculation.IsCalculated)
            return Result.Conflict("Розрахунок заблоковано або його поточні дані недоступні.");
        if (saved.Snapshot.CompositionVersion != current.Value.CompositionVersion
            || saved.Snapshot.SettingsVersionId != current.Value.SettingsVersionId
            || !saved.Snapshot.ReferenceVersionIds.Order().SequenceEqual(current.Value.ReferenceVersionIds.Order()))
            return Result.Conflict("Розрахунок застарів. Збережіть новий розрахунок перед публікацією.");
        var published = new PublishedSewingPrice(Guid.NewGuid(), saved.Id, command.ProductId, command.ArticleId,
            at, command.Actor, saved.Snapshot.Calculation.Prices!);
        await publications.PublishAsync(published, cancellationToken);
        await transactions.CommitAsync(cancellationToken);
        return Result.Success(published);
    }
}
