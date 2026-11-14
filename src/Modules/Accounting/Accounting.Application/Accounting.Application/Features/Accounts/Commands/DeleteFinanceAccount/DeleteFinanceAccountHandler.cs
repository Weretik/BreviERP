using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Balances.Shared;

namespace Accounting.Application.Features.Accounts.Commands.DeleteFinanceAccount;

public sealed class DeleteFinanceAccountHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<DeleteFinanceAccountCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteFinanceAccountCommand command, CancellationToken cancellationToken)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(x => x.Id == FinanceAccountId.From(command.Id), cancellationToken);
        if (account is null) return Result.NotFound(AccountingMessages.AccountNotFound);
        if (account.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);
        if ((await FinanceBalances.UsageAsync(db, account.Id, cancellationToken)).HasHistory)
            return Result.Conflict(AccountingMessages.AccountHasHistory);
        if (await db.BankStatements.AnyAsync(x => x.AccountId == account.Id, cancellationToken))
            return Result.Conflict(AccountingMessages.AccountHasStatements);

        AuditTrail.Add(db, AuditEntity.Account, command.Id, AuditAction.Delete, command.By, clock.GetUtcNow(),
            AuditTrail.Snapshot(account), null);
        db.Accounts.Remove(account);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(AccountingMessages.Stale);
        }

        return Result.Success();
    }
}
