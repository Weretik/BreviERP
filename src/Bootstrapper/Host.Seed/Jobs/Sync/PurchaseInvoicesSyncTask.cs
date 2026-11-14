using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

namespace Host.Seed.Jobs.Sync;

/// <summary>
/// Sheet «финансы» → «Рахунки на закупку», the same run as «Оновити зараз» (PUR-001). A sheet that cannot be read or
/// breaks a rule changes nothing; the step then fails so the job shows it.
/// </summary>
internal sealed class PurchaseInvoicesSyncTask : ISyncTask
{
    public string Name => "purchase-invoices";

    public async Task RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var result = await services.GetRequiredService<PurchaseRegisterSync>().RunAsync(cancellationToken);
        if (!result.IsSuccess)
            throw new InvalidOperationException(string.Join(" ", result.Errors));
        var value = result.Value;
        logger.LogInformation(
            "Rows {Rows}, files {Files}; messages added {MessagesAdded}, updated {MessagesUpdated}; rows added {RowsAdded}, " +
            "updated {RowsUpdated}, gone {RowsGone}; files added {FilesAdded}, gone {FilesGone}",
            value.RecordCount, value.FileCount, value.MessagesAdded, value.MessagesUpdated, value.RowsAdded, value.RowsUpdated,
            value.RowsDeactivated, value.FilesAdded, value.FilesDeactivated);
    }
}
