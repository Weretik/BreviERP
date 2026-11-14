using Crm.Domain.Leads;
using Crm.Domain.Tasks;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Leads.Shared.Mapping;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Abstractions.Leads;

namespace Crm.Application.Features.Leads.Shared.Calling;

/// <summary>The card of a lead with its category text and open «Подзвонити» task.</summary>
internal static class LeadDetailLoader
{
    public static async Task<LeadDetailResponse> DetailAsync(this ILeadStore store, Lead lead, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var description = lead.Category is null
            ? null
            : await store.LeadCategories.AsNoTracking().Where(x => x.Code == lead.Category).Select(x => x.Description)
                .SingleOrDefaultAsync(cancellationToken);
        var taskId = await store.ManagerTasks.AsNoTracking()
            .Where(x => x.LeadId == lead.Id && x.Status != ManagerTaskStatus.Done)
            .OrderByDescending(x => x.Id).Select(x => (long?)x.Id).FirstOrDefaultAsync(cancellationToken);
        return lead.ToDetail(description, taskId, now);
    }
}
