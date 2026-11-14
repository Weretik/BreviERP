using Crm.Domain.Leads;
using Crm.Domain.Tasks;

namespace Crm.Application.Features.Leads.Shared.Calling;

/// <summary>The «Подзвонити» task of a call with a next contact (R-08), for a call in Brevi or one from the sheet.</summary>
public static class LeadCallTask
{
    public static ManagerTask Create(Lead lead, LeadCall call, ManagerTaskPerson assignee, DateTimeOffset dueAt,
        DateTimeOffset now) =>
        ManagerTask.ForLeadCall(lead.Id, Draft(lead, call, assignee, dueAt), assignee, now);

    private static ManagerTaskDraft Draft(Lead lead, LeadCall call, ManagerTaskPerson assignee, DateTimeOffset dueAt)
    {
        var lines = new List<string> { $"Лід № {lead.SourceId} · останній дзвінок: {call.ResultName}." };
        if (call.Comment is not null) lines.Add(call.Comment);
        if (lead.CallReason is not null) lines.Add($"Привід для дзвінка: {lead.CallReason}");
        var title = LeadLimits.Cut($"Подзвонити: {lead.Company}", ManagerTaskLimits.TitleMaxLength)!;
        var description = LeadLimits.Cut(string.Join("\n", lines), ManagerTaskLimits.DescriptionMaxLength);
        return new ManagerTaskDraft(title, description, assignee, dueAt, false, null, null, []);
    }
}
