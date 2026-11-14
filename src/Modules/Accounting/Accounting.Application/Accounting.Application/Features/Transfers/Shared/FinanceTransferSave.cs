using Accounting.Application.Abstractions;
using Accounting.Domain.Transfers;
using Accounting.Application;
using Accounting.Application.Features.Lookup.Shared;

namespace Accounting.Application.Features.Transfers.Shared;

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
