using Accounting.Domain.Accounts;
using Accounting.Domain.Errors;
using Accounting.Domain.Movements;

namespace Accounting.Domain.Transfers;

/// <summary>Editable fields of a transfer between own accounts, already cleaned by <see cref="Of"/>.</summary>
public sealed record FinanceTransferDetails(
    DateOnly Date,
    FinanceAccountId FromAccountId,
    FinanceAccountId ToAccountId,
    long AmountMinor,
    string Note)
{
    public const int NoteMaxLength = 1500;

    public static DomainResult<FinanceTransferDetails> Of(DateOnly date, FinanceAccountId fromAccountId, FinanceAccountId toAccountId,
        long amountMinor, string? note)
    {
        if (fromAccountId == toAccountId)
            return AccountingErrors.Rule("Accounting.Transfer.Accounts", "Рахунки списання і зарахування мають бути різними.");
        var amount = MovementRules.CheckAmount(amountMinor);
        if (amount.IsFailure) return amount.Error!;
        var cleanNote = MovementRules.Text(note, NoteMaxLength, "Примітка");
        if (cleanNote.IsFailure) return cleanNote.Error!;
        return new FinanceTransferDetails(date, fromAccountId, toAccountId, amountMinor, cleanNote.Value);
    }
}
