using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.UpdateFinanceCounterparty;

/// <summary>«Змінити» a finance counterparty; a stale <paramref name="Version"/> gets 409. Operations keep their link.</summary>
public sealed record UpdateFinanceCounterpartyCommand(Guid Id, FinanceCounterpartyFields Fields, int Version, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;

public sealed class UpdateFinanceCounterpartyValidator : AbstractValidator<UpdateFinanceCounterpartyCommand>
{
    public UpdateFinanceCounterpartyValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceCounterpartyFieldsValidator());
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class UpdateFinanceCounterpartyHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateFinanceCounterpartyCommand, Result<FinanceCounterpartyResponse>>
{
    public async ValueTask<Result<FinanceCounterpartyResponse>> Handle(UpdateFinanceCounterpartyCommand command,
        CancellationToken cancellationToken)
    {
        var details = command.Fields.ToDetails();
        if (details.IsFailure) return details.Error!.ToResult<FinanceCounterpartyResponse>();
        var (category, missing) = await command.Fields.LoadCategoryAsync(db, cancellationToken);
        if (missing) return Result.NotFound(AccountingMessages.DefaultCategoryNotFound);
        var now = clock.GetUtcNow();
        return await FinanceCounterpartyChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            counterparty => Task.FromResult(counterparty.Update(details.Value, category, now)
                .Map<AuditAction?>(changed => changed ? AuditAction.Update : null)),
            cancellationToken);
    }
}
