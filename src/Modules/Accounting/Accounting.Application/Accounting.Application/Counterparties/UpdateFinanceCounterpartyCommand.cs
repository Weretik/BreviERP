using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Application.Journal;
using Accounting.Domain.Audit;

namespace Accounting.Application.Counterparties;

/// <summary>«Змінити» a finance counterparty; a stale <paramref name="Version"/> gets 409. Operations keep their link.</summary>
public sealed record UpdateFinanceCounterpartyCommand(Guid Id, FinanceCounterpartyFields Fields, int Version, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;

public sealed class UpdateFinanceCounterpartyCommandValidator : AbstractValidator<UpdateFinanceCounterpartyCommand>
{
    public UpdateFinanceCounterpartyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceCounterpartyFieldsValidator());
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class UpdateFinanceCounterpartyCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateFinanceCounterpartyCommand, Result<FinanceCounterpartyResponse>>
{
    public async ValueTask<Result<FinanceCounterpartyResponse>> Handle(UpdateFinanceCounterpartyCommand command,
        CancellationToken cancellationToken)
    {
        var details = command.Fields.ToDetails();
        var (category, missing) = await command.Fields.LoadCategoryAsync(db, cancellationToken);
        if (missing) return Result.NotFound(AccountingMessages.DefaultCategoryNotFound);
        var now = clock.GetUtcNow();
        return await FinanceCounterpartyChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            counterparty => Task.FromResult<AuditAction?>(counterparty.Update(details, category, now) ? AuditAction.Update : null),
            cancellationToken);
    }
}
