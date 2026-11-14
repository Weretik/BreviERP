using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Transfers;
using Accounting.Application;

namespace Accounting.Application.Features.Transfers.Shared;

/// <summary>Transfer form as sent by the admin; cleaned and checked by the domain.</summary>
public sealed record FinanceTransferFields(DateOnly Date, Guid FromAccountId, Guid ToAccountId, long AmountMinor, string? Note)
{
    public DomainResult<FinanceTransferDetails> ToDetails() => FinanceTransferDetails.Of(Date, FinanceAccountId.From(FromAccountId),
        FinanceAccountId.From(ToAccountId), AmountMinor, Note);

    /// <summary>Both accounts; <c>Missing</c> is the message when one of them is gone.</summary>
    internal async Task<(FinanceAccount From, FinanceAccount To, string? Missing)> LoadAsync(IAccountingDbContext db,
        CancellationToken cancellationToken)
    {
        var from = await db.Accounts.FirstOrDefaultAsync(x => x.Id == FinanceAccountId.From(FromAccountId), cancellationToken);
        if (from is null) return (null!, null!, AccountingMessages.FromAccountNotFound);
        var to = await db.Accounts.FirstOrDefaultAsync(x => x.Id == FinanceAccountId.From(ToAccountId), cancellationToken);
        return to is null ? (from, null!, AccountingMessages.ToAccountNotFound) : (from, to, null);
    }
}

public sealed class FinanceTransferFieldsValidator : AbstractValidator<FinanceTransferFields>
{
    public FinanceTransferFieldsValidator()
    {
        RuleFor(x => x.FromAccountId).NotEmpty().WithMessage("Оберіть рахунок списання.");
        RuleFor(x => x.ToAccountId).NotEmpty().WithMessage("Оберіть рахунок зарахування.");
        RuleFor(x => x.AmountMinor).GreaterThan(0).WithMessage("Сума має бути більшою за нуль.");
        RuleFor(x => x.Note).MaximumLength(FinanceTransferDetails.NoteMaxLength + 20);
    }
}
