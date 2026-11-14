using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Counterparties;
using Accounting.Application.Journal;
using Accounting.Application.Lookup;
using Accounting.Domain.Audit;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Transactions;

/// <summary>
/// «Анулювати»: the operation is never deleted; it stays in the journal («Показати анульовані») and in «Історія» but no
/// longer counts in balances and totals. There is no way back for a hand-made operation.
/// </summary>
public sealed record VoidFinanceTransactionCommand(Guid Id, int Version, string By)
    : ICommand<Result<FinanceTransactionResponse>>;

public sealed class VoidFinanceTransactionCommandValidator : AbstractValidator<VoidFinanceTransactionCommand>
{
    public VoidFinanceTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class VoidFinanceTransactionCommandHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers, TimeProvider clock)
    : ICommandHandler<VoidFinanceTransactionCommand, Result<FinanceTransactionResponse>>
{
    public async ValueTask<Result<FinanceTransactionResponse>> Handle(VoidFinanceTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions.FirstOrDefaultAsync(x => x.Id == FinanceTransactionId.From(command.Id),
            cancellationToken);
        if (transaction is null) return Result.NotFound(AccountingMessages.TransactionNotFound);
        if (transaction.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);

        var names = (await FinanceNames.LoadAsync(db, cancellationToken)).With(await CounterpartyStates.LoadAsync(
            new CounterpartyReaders(db, customers, suppliers), [transaction.Counterparty], cancellationToken));
        var before = AuditTrail.Snapshot(transaction, names);
        var now = clock.GetUtcNow();
        var voided = transaction.Void(now);
        if (voided.IsFailure) return voided.Error!.ToResult<FinanceTransactionResponse>();
        AuditTrail.Add(db, AuditEntity.Transaction, command.Id, AuditAction.Void, command.By, now, before,
            AuditTrail.Snapshot(transaction, names));
        return await FinanceTransactionSave.SaveAsync(db, transaction, names, cancellationToken);
    }
}
