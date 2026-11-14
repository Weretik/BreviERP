using Microsoft.Extensions.Logging;
using Crm.Application.Features.Leads.Commands.SyncLeads;

namespace Host.Seed.Jobs.Tasks.Office;

/// <summary>
/// Sheet «Лиды Brevi» → «Ліди», the same run as «Синхронізувати зараз» (LEAD-001 R-03). A sheet that cannot be read or
/// breaks a rule changes nothing; the task then fails so the admin page shows it.
/// </summary>
internal sealed class LeadsTaskRunner : IScheduledTaskRunner
{
    public string Key => "leads";

    public async Task<string> RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var value = await services.SendAsync(new SyncLeadsCommand(), cancellationToken);
        logger.LogInformation(
            "Lead rows {Rows}; added {Added}, updated {Updated}, gone {Gone}, skipped {Skipped}, taken for managers {Assigned}, calls from the sheet {Calls}",
            value.Rows, value.Added, value.Updated, value.Gone, value.Skipped, value.Assigned, value.Calls);
        return $"Рядків {value.Rows}: додано {value.Added}, оновлено {value.Updated}, зникло {value.Gone}, пропущено {value.Skipped}.";
    }
}
