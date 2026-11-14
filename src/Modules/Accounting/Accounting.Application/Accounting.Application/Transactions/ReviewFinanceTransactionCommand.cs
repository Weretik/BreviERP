using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Counterparties;
using Accounting.Application.Journal;
using Accounting.Application.Lookup;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Transactions;

/// <summary>
/// «Перевірити» an operation of «Потребують перевірки»: the category and who it is with, nothing else. A finance
/// counterparty must be confirmed first; a stale <paramref name="Version"/> gets 409. <paramref name="RememberCategory"/>
/// makes the category the default of the chosen finance counterparty and applies it to its next operations; it does
/// nothing for a client, a supplier or a one-off name.
/// </summary>
public sealed record ReviewFinanceTransactionCommand(Guid Id, Guid CategoryId, FinanceCounterpartyLinkInput? Counterparty,
    string? OneoffName, int Version, string By, bool RememberCategory = false)
    : ICommand<Result<FinanceTransactionResponse>>;

public sealed class ReviewFinanceTransactionCommandValidator : AbstractValidator<ReviewFinanceTransactionCommand>
{
    public ReviewFinanceTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Оберіть категорію.");
        RuleFor(x => x.Counterparty!).SetValidator(new FinanceCounterpartyLinkInputValidator()).When(x => x.Counterparty is not null);
        RuleFor(x => x.OneoffName).MaximumLength(FinanceTransactionDetails.OneoffNameMaxLength + 20);
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class ReviewFinanceTransactionCommandHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers, TimeProvider clock)
    : ICommandHandler<ReviewFinanceTransactionCommand, Result<FinanceTransactionResponse>>
{
    public async ValueTask<Result<FinanceTransactionResponse>> Handle(ReviewFinanceTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var picked = FinanceCounterpartyLinkInput.LinkOf(command.Counterparty);
        if (picked.IsFailure) return picked.Error!.ToResult<FinanceTransactionResponse>();
        var link = picked.Value;
        var transaction = await db.Transactions.FirstOrDefaultAsync(x => x.Id == FinanceTransactionId.From(command.Id),
            cancellationToken);
        if (transaction is null) return Result.NotFound(AccountingMessages.TransactionNotFound);
        if (transaction.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == FinanceCategoryId.From(command.CategoryId),
            cancellationToken);
        if (category is null) return Result.NotFound(AccountingMessages.CategoryNotFound);

        var states = await CounterpartyStates.LoadAsync(new CounterpartyReaders(db, customers, suppliers),
            [link, transaction.Counterparty], cancellationToken);
        var counterparty = states.Of(link);
        if (link is not null && counterparty is null) return Result.NotFound(AccountingMessages.CounterpartyNotFound);

        var names = (await FinanceNames.LoadAsync(db, cancellationToken)).With(states);
        var before = AuditTrail.Snapshot(transaction, names);
        var now = clock.GetUtcNow();
        var review = transaction.Review(category, counterparty, command.OneoffName, now);
        if (review.IsFailure) return review.Error!.ToResult<FinanceTransactionResponse>();
        var reviewed = review.Value;
        if (reviewed)
            AuditTrail.Add(db, AuditEntity.Transaction, command.Id, AuditAction.Review, command.By, now, before,
                AuditTrail.Snapshot(transaction, names));
        var remembered = false;
        if (command.RememberCategory && transaction.Counterparty is { Kind: CounterpartyKind.Financial } chosen)
        {
            var remember = await RememberAsync(Guid.Parse(chosen.Id), category, names, command.By, now, cancellationToken);
            if (remember.IsFailure) return remember.Error!.ToResult<FinanceTransactionResponse>();
            remembered = remember.Value;
        }

        if (!reviewed && !remembered) return Result.Success(FinanceTransactionResponse.From(transaction, names));
        return await FinanceTransactionSave.SaveAsync(db, transaction, names, cancellationToken);
    }

    /// <summary>«Запамʼятати»: the category becomes the counterparty's default and applies to its next operations.</summary>
    private async Task<DomainResult<bool>> RememberAsync(Guid counterpartyId, FinanceCategory category, FinanceNames names, string by,
        DateTimeOffset now, CancellationToken cancellationToken)
    {
        var counterparty = await FinanceCounterpartyChange.FindAsync(db, counterpartyId, cancellationToken);
        if (counterparty is null || counterparty.MergedIntoId is not null) return false;
        var before = AuditTrail.Snapshot(counterparty, names);
        var details = counterparty.Details with { DefaultCategoryId = category.Id, AutoApplyCategory = true };
        var updated = counterparty.Update(details, category, now);
        if (updated.IsFailure || !updated.Value) return updated;
        AuditTrail.Add(db, AuditEntity.Counterparty, counterpartyId, AuditAction.Update, by, now, before,
            AuditTrail.Snapshot(counterparty, names));
        return true;
    }
}
