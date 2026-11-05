using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Application.Journal;
using Accounting.Domain.Audit;

namespace Accounting.Application.Counterparties;

/// <summary>
/// «Архівувати» (<paramref name="Active"/> false) or «Відновити» (true). An archived counterparty cannot be chosen for
/// new operations; the ones that already have it keep the link.
/// </summary>
public sealed record SetFinanceCounterpartyActiveCommand(Guid Id, bool Active, int Version, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;

public sealed class SetFinanceCounterpartyActiveCommandValidator : AbstractValidator<SetFinanceCounterpartyActiveCommand>
{
    public SetFinanceCounterpartyActiveCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class SetFinanceCounterpartyActiveCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<SetFinanceCounterpartyActiveCommand, Result<FinanceCounterpartyResponse>>
{
    public async ValueTask<Result<FinanceCounterpartyResponse>> Handle(SetFinanceCounterpartyActiveCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        return await FinanceCounterpartyChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            counterparty => Task.FromResult<AuditAction?>(counterparty.SetActive(command.Active, now)
                ? command.Active ? AuditAction.Restore : AuditAction.Archive
                : null),
            cancellationToken);
    }
}
