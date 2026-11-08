using Crm.Application.Leads.Access;
using Crm.Application.Leads.Contracts;
using Crm.Application.Leads.Mapping;
using Crm.Application.Leads.Persistence;
using Crm.Domain.Leads;

namespace Crm.Application.Leads.References;

/// <summary>
/// «Довідники › Результати дзвінка» (R-11): adds (<paramref name="Id"/> null) or changes a result button. A result is
/// never deleted, only switched off, so the calls recorded with it keep their text.
/// </summary>
public sealed record SaveLeadCallResultCommand(long? Id, SaveLeadCallResultInput Input)
    : ICommand<Result<LeadCallResultResponse>>;

public sealed class SaveLeadCallResultCommandValidator : AbstractValidator<SaveLeadCallResultCommand>
{
    public SaveLeadCallResultCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id is not null);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.Name).NotEmpty().WithMessage("Вкажіть назву результату.")
                .MaximumLength(LeadLimits.ResultNameMaxLength)
                .WithMessage($"Назва до {LeadLimits.ResultNameMaxLength} символів.");
            RuleFor(x => x.Input.Outcome).Must(x => Enum.TryParse<LeadOutcome>(x, out _))
                .WithMessage("Невідома дія результату.");
            RuleFor(x => x.Input.RetryDays).InclusiveBetween(1, LeadLimits.MaxRetryDays)
                .When(x => x.Input.RetryDays is not null)
                .WithMessage($"Повтор через 1–{LeadLimits.MaxRetryDays} днів.");
            RuleFor(x => x.Input.Position).InclusiveBetween(0, 1000);
        });
    }
}

public sealed class SaveLeadCallResultCommandHandler(ILeadStore store)
    : ICommandHandler<SaveLeadCallResultCommand, Result<LeadCallResultResponse>>
{
    public async ValueTask<Result<LeadCallResultResponse>> Handle(SaveLeadCallResultCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        var outcome = Enum.Parse<LeadOutcome>(input.Outcome);
        LeadCallResult result;
        if (command.Id is { } id)
        {
            var found = await store.LeadCallResults.FindAsync([id], cancellationToken);
            if (found is null) return Result.NotFound(LeadMessages.ResultNotFound);
            result = found;
            result.Edit(input.Name, outcome, input.RetryDays, input.IsActive, input.Position);
        }
        else
        {
            if (await store.LeadCallResults.CountAsync(cancellationToken) >= LeadLimits.MaxResults)
                return Result.Invalid(new ValidationError(LeadMessages.TooManyResults));
            result = new LeadCallResult(input.Name, outcome, input.RetryDays, input.Position);
            if (!input.IsActive) result.Edit(input.Name, outcome, input.RetryDays, false, input.Position);
            store.LeadCallResults.Add(result);
        }

        await store.SaveChangesAsync(cancellationToken);
        return command.Id is null ? Result.Created(result.ToResponse()) : Result.Success(result.ToResponse());
    }
}
