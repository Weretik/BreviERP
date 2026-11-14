using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Domain.Ledger;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Commands.CreateFinanceTransaction;

public sealed class CreateFinanceTransactionHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers, TimeProvider clock)
    : ICommandHandler<CreateFinanceTransactionCommand, Result<FinanceTransactionResponse>>
{
    public async ValueTask<Result<FinanceTransactionResponse>> Handle(CreateFinanceTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var fields = command.Fields.ToDetails();
        if (fields.IsFailure) return fields.Error!.ToResult<FinanceTransactionResponse>();
        var details = fields.Value;
        var (account, category, missing) = await command.Fields.LoadAsync(db, cancellationToken);
        if (missing is not null) return Result.NotFound(missing);
        var readers = new CounterpartyReaders(db, customers, suppliers);
        var (counterparty, gone) = await CounterpartyStates.ResolveAsync(readers, details.Counterparty, cancellationToken);
        if (gone is not null) return Result.NotFound(gone);

        var now = clock.GetUtcNow();
        var created = FinanceTransaction.Create(FinanceTransactionId.New(), command.Scope, details,
            account, category, counterparty, command.By, now);
        if (created.IsFailure) return created.Error!.ToResult<FinanceTransactionResponse>();
        var transaction = created.Value;
        db.Transactions.Add(transaction);
        var names = (await FinanceNames.LoadAsync(db, cancellationToken))
            .With(await CounterpartyStates.LoadAsync(readers, [details.Counterparty], cancellationToken));
        AuditTrail.Add(db, AuditEntity.Transaction, transaction.Id.Value, AuditAction.Create, command.By, now, null,
            AuditTrail.Snapshot(transaction, names));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(FinanceTransactionResponse.From(transaction, names));
    }
}
