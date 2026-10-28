using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Evaluation;

namespace Catalog.Application.Features.Pricing.Sewing.Publications.Publish;

public sealed class PublishSewingCalculationCommandHandler(
    ISewingPricingTransaction transactions, ISewingCalculationStore calculations,
    ISewingPublicationStore publications, ISewingPricingInputReader inputs,
    ISewingPricingSettingsReader settings, IPricingReferenceVersionReader references)
    : ICommandHandler<PublishSewingCalculationCommand, Result<PublishedSewingPrice>>
{
    public async ValueTask<Result<PublishedSewingPrice>> Handle(
        PublishSewingCalculationCommand command, CancellationToken cancellationToken)
    {
        if (command.ProductId <= 0 || command.ArticleId <= 0 || command.CalculationId == Guid.Empty
            || string.IsNullOrWhiteSpace(command.Actor) || command.Actor.Length > 200)
            return Result.Invalid(new ValidationError("request", "Positive IDs and authenticated actor are required."));
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
            return Result.Conflict("Calculation is blocked or its current inputs are unavailable.");
        if (saved.Snapshot.CompositionVersion != current.Value.CompositionVersion
            || saved.Snapshot.SettingsVersionId != current.Value.SettingsVersionId
            || !saved.Snapshot.ReferenceVersionIds.Order().SequenceEqual(current.Value.ReferenceVersionIds.Order()))
            return Result.Conflict("Calculation is stale. Save a new calculation before publishing.");
        var published = new PublishedSewingPrice(Guid.NewGuid(), saved.Id, command.ProductId, command.ArticleId,
            at, command.Actor, saved.Snapshot.Calculation.Prices!);
        await publications.PublishAsync(published, cancellationToken);
        await transactions.CommitAsync(cancellationToken);
        return Result.Success(published);
    }
}
