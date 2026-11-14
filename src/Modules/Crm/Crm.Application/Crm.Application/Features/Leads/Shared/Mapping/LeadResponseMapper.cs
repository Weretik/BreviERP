using Crm.Domain.Leads;
using Crm.Application.Features.Leads.Shared;
using Crm.Domain.Leads.Enums;

namespace Crm.Application.Features.Leads.Shared.Mapping;

internal static class LeadResponseMapper
{
    /// <summary>SQL-translatable projection for lists and «Мій день».</summary>
    public static IQueryable<LeadSummaryResponse> ToSummaries(this IQueryable<Lead> leads, DateTimeOffset now) =>
        leads.Select(x => new LeadSummaryResponse(
            x.Id,
            x.SourceId,
            x.Company,
            x.Category,
            x.Region,
            x.Industry,
            x.CallReason,
            x.Stage.ToString(),
            x.OwnerUserId == null ? null : new LeadPersonResponse(x.OwnerUserId.Value, x.OwnerName!),
            x.LastResultName,
            x.LastCallAt,
            x.NextContactAt,
            x.NextContactAt != null && x.NextContactAt < now,
            x.CallCount,
            x.AddedOn,
            x.InSheet,
            x.Version));

    public static LeadDetailResponse ToDetail(this Lead lead, string? categoryDescription, long? openTaskId,
        DateTimeOffset now) =>
        new(lead.Id,
            lead.SourceId,
            lead.Company,
            lead.Category,
            categoryDescription,
            lead.AddedOn,
            lead.Region,
            lead.Industry,
            lead.Scale,
            lead.Signal,
            lead.SignalDate,
            lead.FactOrPlan,
            lead.SignalUrl,
            lead.VolumeReason,
            lead.Offer,
            lead.Website,
            lead.Contacts,
            lead.WhomToCall,
            lead.TenderCheck,
            lead.CallReason,
            lead.Reliability,
            lead.Email,
            lead.SheetNotes,
            lead.InSheet,
            lead.Stage.ToString(),
            lead.OwnerUserId is { } owner ? new LeadPersonResponse(owner, lead.OwnerName!) : null,
            lead.TakenAt,
            lead.LastResultName,
            lead.LastCallAt,
            lead.NextContactAt,
            lead.NextContactAt < now,
            lead.CallCount,
            openTaskId,
            lead.Calls.OrderByDescending(x => x.At).ThenByDescending(x => x.Id)
                .Select(x => new LeadCallResponse(x.Id, new LeadPersonResponse(x.ByUserId, x.ByName), x.At, x.ResultId,
                    x.ResultName, x.Outcome.ToString(), x.Comment, x.NextContactAt))
                .ToArray(),
            lead.Version);

    public static LeadCallResultResponse ToResponse(this LeadCallResult x) =>
        new(x.Id, x.Name, x.Outcome.ToString(), x.RetryDays, x.IsActive, x.Position);
}
