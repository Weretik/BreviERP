using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Accounts.Shared;
using Accounting.Application.Features.Balances.Shared;
using BuildingBlocks.Application;

namespace Accounting.Application.Features.Accounts.Commands.UpdateFinanceAccount;

public sealed class UpdateFinanceAccountHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateFinanceAccountCommand, Result<FinanceAccountResponse>>
{
    public async ValueTask<Result<FinanceAccountResponse>> Handle(UpdateFinanceAccountCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var details = command.Fields.ToDetails();
        if (details.IsFailure) return details.Error!.ToResult<FinanceAccountResponse>();
        var usage = await FinanceBalances.UsageAsync(db, FinanceAccountId.From(command.Id), cancellationToken);
        return await FinanceAccountChange.ApplyAsync(db, command.Id, command.Version, command.By, now,
            account => account.Update(details.Value, usage, now).Map<AuditAction?>(changed => changed ? AuditAction.Update : null),
            cancellationToken);
    }
}
