using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Application.Features.Accounts.Commands.SetFinanceAccountActive;

public sealed class SetFinanceAccountActiveHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<SetFinanceAccountActiveCommand, Result<FinanceAccountResponse>>
{
    public async ValueTask<Result<FinanceAccountResponse>> Handle(SetFinanceAccountActiveCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        return await FinanceAccountChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            account => DomainResult<AuditAction?>.Success(account.SetActive(command.Active, now)
                ? command.Active ? AuditAction.Restore : AuditAction.Archive
                : null),
            cancellationToken);
    }
}
