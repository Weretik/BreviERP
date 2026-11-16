using ActivityLog.Contracts.Activity;
using Reference.Domain;

namespace Reference.Application.Features.Activity.EventHandlers;

/// <summary>
/// How «Довідники» entries read in «Журнал дій» (ACT-001): created, changed (name and price as «was → became»),
/// deleted. Imports and the KeepinCRM supplier sync are not listed.
/// </summary>
public sealed class ReferenceActivity : IActivityDescriber
{
    public ActivityDescription? Describe(object domainEvent)
    {
        if (domainEvent is not ReferenceItemRecorded e || ReferenceSubject.Of(e.Item) is not { } subject) return null;
        var action = e.Action switch
        {
            ReferenceItemAction.Created => ActivityActions.Created,
            ReferenceItemAction.Deleted => ActivityActions.Deleted,
            _ => ActivityActions.Changed,
        };
        var changes = e.Action == ReferenceItemAction.Changed
            ? new ActivityChanges()
                .Add("Назва", e.NameBefore ?? Plain(subject), Plain(subject))
                .Add("Ціна", e.PriceBefore ?? subject.Price, subject.Price, "грн").Items
            : [];
        return new ActivityDescription(action, subject.Key, subject.Title, subject.Id, subject.Name, null, changes,
            "Reference");
    }

    /// <summary>The name without the code, as <see cref="ReferenceItemRecorded.NameBefore"/> holds it.</summary>
    private static string Plain(ReferenceSubject subject)
    {
        var dot = subject.Name.IndexOf(" · ", StringComparison.Ordinal);
        return dot < 0 ? subject.Name : subject.Name[(dot + 3)..];
    }
}
