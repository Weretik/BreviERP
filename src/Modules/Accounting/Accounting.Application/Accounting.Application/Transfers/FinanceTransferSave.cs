using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Application.Lookup;
using Accounting.Domain.Transfers;

namespace Accounting.Application.Transfers;

internal static class FinanceTransferSave
{
    /// <summary>Saves; a concurrent save of the same version gives 409.</summary>
    public static async Task<Result<FinanceTransferResponse>> SaveAsync(IAccountingDbContext db, FinanceTransfer transfer,
        FinanceNames names, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(AccountingMessages.Stale);
        }

        return Result.Success(FinanceTransferResponse.From(transfer, names));
    }
}
