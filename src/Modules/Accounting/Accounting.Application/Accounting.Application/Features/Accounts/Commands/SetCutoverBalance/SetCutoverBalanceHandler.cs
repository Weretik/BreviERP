using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Application.Features.Accounts.Commands.SetCutoverBalance;

public sealed class SetCutoverBalanceHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<SetCutoverBalanceCommand, Result<FinanceAccountResponse>>
{
    public async ValueTask<Result<FinanceAccountResponse>> Handle(SetCutoverBalanceCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        return await FinanceAccountChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            account => account.SetCutoverBalance(command.BalanceMinor, command.Confirmed, command.By, now)
                .Map<AuditAction?>(changed => changed
                    ? command.Confirmed ? AuditAction.CutoverConfirm : AuditAction.CutoverReset
                    : null),
            cancellationToken);
    }
}
