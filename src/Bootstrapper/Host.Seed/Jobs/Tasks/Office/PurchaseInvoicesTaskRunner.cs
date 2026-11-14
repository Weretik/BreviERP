using Microsoft.Extensions.Logging;
using Purchasing.Application.Features.Register.Commands.SyncPurchaseRegister;

namespace Host.Seed.Jobs.Tasks.Office;

/// <summary>
/// Sheet «финансы» → «Рахунки на закупку», the same run as «Оновити зараз» (PUR-001). A sheet that cannot be read or
/// breaks a rule changes nothing; the task then fails so the admin page shows it.
/// </summary>
internal sealed class PurchaseInvoicesTaskRunner : IScheduledTaskRunner
{
    public string Key => "purchase-invoices";

    public async Task<string> RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var value = await services.SendAsync(new SyncPurchaseRegisterCommand(), cancellationToken);
        logger.LogInformation(
            "Rows {Rows}, files {Files}; messages added {MessagesAdded}, updated {MessagesUpdated}; rows added {RowsAdded}, " +
            "updated {RowsUpdated}, gone {RowsGone}; files added {FilesAdded}, gone {FilesGone}",
            value.RecordCount, value.FileCount, value.MessagesAdded, value.MessagesUpdated, value.RowsAdded, value.RowsUpdated,
            value.RowsDeactivated, value.FilesAdded, value.FilesDeactivated);
        return $"Рядків {value.RecordCount}: додано {value.RowsAdded}, оновлено {value.RowsUpdated}, зникло {value.RowsDeactivated}; " +
               $"файлів додано {value.FilesAdded}.";
    }
}
