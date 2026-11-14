using Crm.Domain.Embroidery.Saved;
using Crm.Application.Features.Embroidery.Queries.CalculateEmbroidery;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;
using Crm.Application.Abstractions.Embroidery;
using Crm.Application.Abstractions.Quotes;
using BuildingBlocks.Application;

namespace Crm.Application.Features.Embroidery.Commands.SaveEmbroideryCalculation;

public sealed class SaveEmbroideryCalculationHandler(
    IEmbroideryStore store,
    IEmbroideryStitchRateReader stitchRate,
    IQuoteStaffReader staff,
    TimeProvider clock)
    : ICommandHandler<SaveEmbroideryCalculationCommand, Result<EmbroiderySavedCalculationResponse>>
{
    public async ValueTask<Result<EmbroiderySavedCalculationResponse>> Handle(SaveEmbroideryCalculationCommand command,
        CancellationToken cancellationToken)
    {
        var actor = await staff.FindAsync(command.Actor.UserId, cancellationToken);
        if (actor is null) return Result.Forbidden(EmbroideryMessages.ActorUnknown);

        string? customerName = null;
        if (command.Input.CustomerPublicId is { } customerId)
        {
            customerName = await store.FindCustomerNameAsync(customerId, cancellationToken);
            if (customerName is null) return Result.Invalid(new ValidationError(EmbroideryMessages.CustomerNotFound));
        }

        var input = command.Input.Calculation.ToDomain();
        var priced = await EmbroideryPricing.PriceAsync(store, stitchRate, input, cancellationToken);
        if (priced.Calculation.Result is not { } result || priced.Snapshot is not { } snapshot)
            return Result.Invalid(new ValidationError(priced.Calculation.Error!));
        var created = EmbroiderySavedCalculation.Create(command.Input.Name, command.Input.CustomerPublicId, customerName,
            input, snapshot, result, actor.UserId, actor.Name, clock.GetUtcNow(), command.Input.Category.Trim());
        if (created.IsFailure) return created.Error!.ToResult<EmbroiderySavedCalculationResponse>();
        var saved = created.Value;
        store.EmbroideryCalculations.Add(saved);
        await store.SaveChangesAsync(cancellationToken);
        return Result.Success(saved.ToResponse(command.Actor.SeesCost));
    }
}
