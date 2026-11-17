using ActivityLog.Contracts.Activity;
using Crm.Domain.Leads;
using Crm.Domain.Leads.Events;

namespace Crm.Application.Features.Activity.EventHandlers;

/// <summary>
/// How lead actions read in «Журнал дій» (ACT-001): taking and giving back a lead, recorded calls, and the
/// director's call results and category descriptions.
/// </summary>
public sealed class LeadActivity : IActivityDescriber
{
    public const string Subject = "lead";
    public const string ResultSubject = "lead-call-result";
    public const string CategorySubject = "lead-category";

    public ActivityDescription? Describe(object domainEvent) => domainEvent switch
    {
        LeadTaken e => Lead(e.Lead, ActivityActions.Changed, "Взято в роботу", []),
        LeadReleased e => Lead(e.Lead, ActivityActions.Changed, "Повернуто у вільні (був у " + e.FormerOwner + ")", []),
        LeadCallRecorded e => Lead(e.Lead, ActivityActions.Changed, Call(e), []),
        LeadCallResultSaved e => Result(e),
        LeadCategoryDescribed e => new ActivityChanges().Add("Опис", e.Before, e.After) is { Any: true } changes
            ? new ActivityDescription(ActivityActions.Changed, CategorySubject, "Категорія ліда", e.Code,
                "Категорія " + e.Code, null, changes.Items, "Crm")
            : null,
        _ => null,
    };

    private static string Call(LeadCallRecorded e)
    {
        var text = "Дзвінок: " + e.ResultName;
        if (ActivityChanges.Moment(e.NextContactAt) is { } next) text += "; наступний контакт " + next;
        if (!string.IsNullOrWhiteSpace(e.Comment)) text += "; " + e.Comment.Trim();
        return text;
    }

    private static ActivityDescription? Result(LeadCallResultSaved e)
    {
        var result = e.Result;
        var id = result.Id.Value.ToString(CultureInfo.InvariantCulture);
        if (e.NameBefore is null)
            return new ActivityDescription(ActivityActions.Created, ResultSubject, "Результат дзвінка", id, result.Name,
                null, [], "Crm");
        var changes = new ActivityChanges()
            .Add("Назва", e.NameBefore, result.Name)
            .Add("Активний", e.ActiveBefore ?? result.IsActive, result.IsActive);
        return new ActivityDescription(ActivityActions.Changed, ResultSubject, "Результат дзвінка", id, result.Name,
            null, changes.Items, "Crm");
    }

    private static ActivityDescription Lead(Lead lead, string action, string summary,
        IReadOnlyList<ActivityChangeItem> changes) =>
        new(action, Subject, "Лід", lead.Id.Value.ToString(CultureInfo.InvariantCulture), lead.Company, summary, changes,
            "Crm");
}
