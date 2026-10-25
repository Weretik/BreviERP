using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Evaluation;

namespace Catalog.Application.Features.Pricing.Sewing.Calculations.Save;

public sealed class SaveSewingCalculationCommandHandler(
    ISewingPricingTransaction transactions, ISewingCalculationStore calculations, ISewingPricingInputReader inputs,
    ISewingPricingSettingsReader settings, IPricingReferenceVersionReader references)
    : ICommandHandler<SaveSewingCalculationCommand, Result<SavedSewingCalculation>>
{
    public async ValueTask<Result<SavedSewingCalculation>> Handle(
        SaveSewingCalculationCommand command, CancellationToken cancellationToken)
    {
        if (command.ProductId <= 0 || command.ArticleId <= 0 || command.RequestId == Guid.Empty
            || string.IsNullOrWhiteSpace(command.Actor) || command.Actor.Length > 200)
            return Result.Invalid(new ValidationError("request", "Positive IDs, request UUID and authenticated actor are required."));
        await using var transaction = await transactions.BeginAsync(command.ProductId, cancellationToken);
        var previous = await calculations.FindRequestAsync(command.RequestId, cancellationToken);
        if (previous is not null)
            return previous.Snapshot.ProductId == command.ProductId && previous.Snapshot.ArticleId == command.ArticleId
                ? Result.Success(previous) : Result.Conflict("Request UUID was used for another Article.");
        var at = DateTimeOffset.UtcNow;
        var result = await new SewingArticlePricingEvaluator(inputs, settings, references)
            .EvaluateAsync(command.ProductId, command.ArticleId, at, cancellationToken);
        if (!result.IsSuccess)
            return result.Status == ResultStatus.NotFound ? Result.NotFound() : Result.Invalid(result.ValidationErrors);
        var saved = new SavedSewingCalculation(Guid.NewGuid(), command.RequestId, at, command.Actor, result.Value);
        await calculations.SaveAsync(saved, cancellationToken);
        await transactions.CommitAsync(cancellationToken);
        return Result.Success(saved);
    }
}
