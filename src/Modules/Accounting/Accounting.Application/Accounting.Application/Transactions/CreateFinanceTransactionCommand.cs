using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Counterparties;
using Accounting.Application.Journal;
using Accounting.Application.Lookup;
using Accounting.Domain.Audit;
using Accounting.Domain.Ledger;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Transactions;

/// <summary>«Додати дохід» / «Додати витрату» in the current books or in «Домашня бухгалтерія» (<paramref name="Scope"/>).</summary>
public sealed record CreateFinanceTransactionCommand(LedgerScope Scope, FinanceTransactionFields Fields, string By)
    : ICommand<Result<FinanceTransactionResponse>>;

public sealed class CreateFinanceTransactionCommandValidator : AbstractValidator<CreateFinanceTransactionCommand>
{
    public CreateFinanceTransactionCommandValidator()
    {
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceTransactionFieldsValidator());
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class CreateFinanceTransactionCommandHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers, TimeProvider clock)
    : ICommandHandler<CreateFinanceTransactionCommand, Result<FinanceTransactionResponse>>
{
    public async ValueTask<Result<FinanceTransactionResponse>> Handle(CreateFinanceTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var details = command.Fields.ToDetails();
        var (account, category, missing) = await command.Fields.LoadAsync(db, cancellationToken);
        if (missing is not null) return Result.NotFound(missing);
        var readers = new CounterpartyReaders(db, customers, suppliers);
        var (counterparty, gone) = await CounterpartyStates.ResolveAsync(readers, details.Counterparty, cancellationToken);
        if (gone is not null) return Result.NotFound(gone);

        var now = clock.GetUtcNow();
        var transaction = FinanceTransaction.Create(FinanceTransactionId.New(), command.Scope, details,
            account, category, counterparty, command.By, now);
        db.Transactions.Add(transaction);
        var names = (await FinanceNames.LoadAsync(db, cancellationToken))
            .With(await CounterpartyStates.LoadAsync(readers, [details.Counterparty], cancellationToken));
        AuditTrail.Add(db, AuditEntity.Transaction, transaction.Id.Value, AuditAction.Create, command.By, now, null,
            AuditTrail.Snapshot(transaction, names));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(FinanceTransactionResponse.From(transaction, names));
    }
}
