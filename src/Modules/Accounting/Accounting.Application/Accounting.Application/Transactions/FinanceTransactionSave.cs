using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Application.Lookup;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Transactions;

internal static class FinanceTransactionSave
{
    /// <summary>Saves; a concurrent save of the same version gives 409.</summary>
    public static async Task<Result<FinanceTransactionResponse>> SaveAsync(IAccountingDbContext db,
        FinanceTransaction transaction, FinanceNames names, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(AccountingMessages.Stale);
        }

        return Result.Success(FinanceTransactionResponse.From(transaction, names));
    }
}
