using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Calculate;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Mapping;
using Crm.Application.Embroidery.Persistence;
using Crm.Application.Embroidery.StitchRate;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Embroidery.Saved;

namespace Crm.Application.Embroidery.SaveCalculation;

/// <summary>
/// «Зберегти розрахунок»: the server prices the form with the rates in force now (the browser sends no prices),
/// freezes those rates and stores a new record with the one price category chosen. An «Орієнтовна» price can be saved; a price that failed cannot.
/// </summary>
public sealed record SaveEmbroideryCalculationCommand(EmbroideryActor Actor, SaveEmbroideryCalculationInput Input)
    : ICommand<Result<EmbroiderySavedCalculationResponse>>;

public sealed class SaveEmbroideryCalculationCommandHandler(
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

public sealed class SaveEmbroideryCalculationCommandValidator : AbstractValidator<SaveEmbroideryCalculationCommand>
{
    /// <summary>The typed name before trimming; longer names are cut to 120 after trimming.</summary>
    public const int RawNameMaxLength = 1000;

    public SaveEmbroideryCalculationCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Input).NotNull().ChildRules(input =>
        {
            input.RuleFor(x => x.Name).MaximumLength(RawNameMaxLength);
            input.RuleFor(x => x.CustomerPublicId).NotEqual(Guid.Empty).When(x => x.CustomerPublicId is not null);
            input.RuleFor(x => x.Calculation).NotNull().SetValidator(new CalculateEmbroideryInputValidator());
            input.RuleFor(x => x.Category).NotEmpty().WithMessage(EmbroideryMessages.CategoryNotFound)
                .MaximumLength(EmbroiderySavedCalculation.CategoryMaxLength);
        });
    }
}
