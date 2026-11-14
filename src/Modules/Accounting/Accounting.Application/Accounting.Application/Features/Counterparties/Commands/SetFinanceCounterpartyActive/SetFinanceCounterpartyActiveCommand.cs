using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.SetFinanceCounterpartyActive;

/// <summary>
/// «Архівувати» (<paramref name="Active"/> false) or «Відновити» (true). An archived counterparty cannot be chosen for
/// new operations; the ones that already have it keep the link.
/// </summary>
public sealed record SetFinanceCounterpartyActiveCommand(Guid Id, bool Active, int Version, string By)
    : ICommand<Result<FinanceCounterpartyResponse>>;

public sealed class SetFinanceCounterpartyActiveValidator : AbstractValidator<SetFinanceCounterpartyActiveCommand>
{
    public SetFinanceCounterpartyActiveValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class SetFinanceCounterpartyActiveHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<SetFinanceCounterpartyActiveCommand, Result<FinanceCounterpartyResponse>>
{
    public async ValueTask<Result<FinanceCounterpartyResponse>> Handle(SetFinanceCounterpartyActiveCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        return await FinanceCounterpartyChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            counterparty => Task.FromResult(counterparty.SetActive(command.Active, now)
                .Map<AuditAction?>(changed => changed
                    ? command.Active ? AuditAction.Restore : AuditAction.Archive
                    : null)),
            cancellationToken);
    }
}
