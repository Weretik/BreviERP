using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Counterparties;
using Accounting.Application.Journal;
using Accounting.Application.Lookup;
using Accounting.Domain.Audit;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Transactions;

/// <summary>«Змінити» an active hand-made operation; the area stays, a stale <paramref name="Version"/> gets 409.</summary>
public sealed record UpdateFinanceTransactionCommand(Guid Id, FinanceTransactionFields Fields, int Version, string By)
    : ICommand<Result<FinanceTransactionResponse>>;

public sealed class UpdateFinanceTransactionCommandValidator : AbstractValidator<UpdateFinanceTransactionCommand>
{
    public UpdateFinanceTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Fields).NotNull().SetValidator(new FinanceTransactionFieldsValidator());
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class UpdateFinanceTransactionCommandHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers, TimeProvider clock)
    : ICommandHandler<UpdateFinanceTransactionCommand, Result<FinanceTransactionResponse>>
{
    public async ValueTask<Result<FinanceTransactionResponse>> Handle(UpdateFinanceTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var fields = command.Fields.ToDetails();
        if (fields.IsFailure) return fields.Error!.ToResult<FinanceTransactionResponse>();
        var details = fields.Value;
        var transaction = await db.Transactions.FirstOrDefaultAsync(x => x.Id == FinanceTransactionId.From(command.Id),
            cancellationToken);
        if (transaction is null) return Result.NotFound(AccountingMessages.TransactionNotFound);
        if (transaction.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);
        var (account, category, missing) = await command.Fields.LoadAsync(db, cancellationToken);
        if (missing is not null) return Result.NotFound(missing);
        var readers = new CounterpartyReaders(db, customers, suppliers);
        var states = await CounterpartyStates.LoadAsync(readers, [details.Counterparty, transaction.Counterparty],
            cancellationToken);
        var counterparty = states.Of(details.Counterparty);
        if (details.Counterparty is not null && counterparty is null)
            return Result.NotFound(AccountingMessages.CounterpartyNotFound);

        var names = (await FinanceNames.LoadAsync(db, cancellationToken)).With(states);
        var before = AuditTrail.Snapshot(transaction, names);
        var now = clock.GetUtcNow();
        var updated = transaction.Update(details, account, category, counterparty, now);
        if (updated.IsFailure) return updated.Error!.ToResult<FinanceTransactionResponse>();
        if (!updated.Value)
            return Result.Success(FinanceTransactionResponse.From(transaction, names));

        AuditTrail.Add(db, AuditEntity.Transaction, command.Id, AuditAction.Update, command.By, now, before,
            AuditTrail.Snapshot(transaction, names));
        return await FinanceTransactionSave.SaveAsync(db, transaction, names, cancellationToken);
    }
}
