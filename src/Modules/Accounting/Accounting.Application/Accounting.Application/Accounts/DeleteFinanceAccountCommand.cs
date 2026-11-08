using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Balances;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;

namespace Accounting.Application.Accounts;

/// <summary>
/// «Видалити» an account. The log keeps its last snapshot. An account with any movement (cancelled ones too) can only
/// be archived: 409.
/// </summary>
public sealed record DeleteFinanceAccountCommand(Guid Id, int Version, string By) : ICommand<Result>;

public sealed class DeleteFinanceAccountCommandValidator : AbstractValidator<DeleteFinanceAccountCommand>
{
    public DeleteFinanceAccountCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).NotEmpty().MaximumLength(FinanceAccount.ByMaxLength);
    }
}

public sealed class DeleteFinanceAccountCommandHandler(IAccountingDbContext db, TimeProvider clock)
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
