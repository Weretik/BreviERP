using Accounting.Application.Abstractions;
using Accounting.Domain.Transactions;
using Accounting.Application;
using Accounting.Application.Features.Lookup.Shared;

namespace Accounting.Application.Features.Transactions.Shared;

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
