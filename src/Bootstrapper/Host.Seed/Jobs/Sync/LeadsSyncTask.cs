using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Crm.Application.Features.Leads.Shared.Sheet;

namespace Host.Seed.Jobs.Sync;

/// <summary>
/// Sheet «Лиды Brevi» → «Ліди», the same run as «Синхронізувати зараз» (LEAD-001 R-03). A sheet that cannot be read or
/// breaks a rule changes nothing; the step then fails so the job shows it.
/// </summary>
internal sealed class LeadsSyncTask : ISyncTask
{
    public string Name => "leads";

    public async Task RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var result = await services.GetRequiredService<LeadSheetSync>().RunAsync(cancellationToken);
        if (!result.IsSuccess)
            throw new InvalidOperationException(string.Join(" ", result.Errors));
        var value = result.Value;
        logger.LogInformation(
            "Lead rows {Rows}; added {Added}, updated {Updated}, gone {Gone}, skipped {Skipped}, taken for managers {Assigned}, calls from the sheet {Calls}",
            value.Rows, value.Added, value.Updated, value.Gone, value.Skipped, value.Assigned, value.Calls);
    }
}
