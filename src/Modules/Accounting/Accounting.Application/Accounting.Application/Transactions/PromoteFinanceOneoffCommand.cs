using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Counterparties;
using Accounting.Application.Journal;
using Accounting.Application.Lookup;
using Accounting.Domain.Audit;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Transactions;

/// <summary>
/// «Зробити постійним» on a one-off name: a checked finance counterparty is created from it and the operation is linked
/// to it. Other operations with the same name stay as they are (names are never joined by themselves).
/// </summary>
public sealed record PromoteFinanceOneoffCommand(Guid Id, int Version, string By)
    : ICommand<Result<FinanceTransactionResponse>>;

public sealed class PromoteFinanceOneoffCommandValidator : AbstractValidator<PromoteFinanceOneoffCommand>
{
    public PromoteFinanceOneoffCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class PromoteFinanceOneoffCommandHandler(IAccountingDbContext db, TimeProvider clock)
    : ICommandHandler<PromoteFinanceOneoffCommand, Result<FinanceTransactionResponse>>
{
    public async ValueTask<Result<FinanceTransactionResponse>> Handle(PromoteFinanceOneoffCommand command,
        CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions.FirstOrDefaultAsync(x => x.Id == FinanceTransactionId.From(command.Id),
            cancellationToken);
        if (transaction is null) return Result.NotFound(AccountingMessages.TransactionNotFound);
        if (transaction.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);

        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        var before = AuditTrail.Snapshot(transaction, names);
        var now = clock.GetUtcNow();
        var counterparty = FinanceCounterparty.Create(FinanceCounterpartyId.New(),
            FinanceCounterpartyDetails.Of(transaction.OneoffToPromote(), null, null, null, null, null, false, null), null,
            verified: true, now);
        transaction.PromoteOneoff(counterparty.State, now);
        db.Counterparties.Add(counterparty);
        names.With(new CounterpartyStates(new Dictionary<CounterpartyLink, CounterpartyState>
        {
            [counterparty.Link] = counterparty.State,
        }));

        AuditTrail.Add(db, AuditEntity.Counterparty, counterparty.Id.Value, AuditAction.Create, command.By, now, null,
            AuditTrail.Snapshot(counterparty, names));
        AuditTrail.Add(db, AuditEntity.Transaction, command.Id, AuditAction.Promote, command.By, now, before,
            AuditTrail.Snapshot(transaction, names));
        return await FinanceTransactionSave.SaveAsync(db, transaction, names, cancellationToken);
    }
}
