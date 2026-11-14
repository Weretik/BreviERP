using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Counterparties.Commands.SetFinanceCounterpartyActive;

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
