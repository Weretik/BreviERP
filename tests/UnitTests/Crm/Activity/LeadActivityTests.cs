using BuildingBlocks.Domain.Identifiers.People;
using ActivityLog.Contracts.Activity;
using BuildingBlocks.Domain.Events;
using Crm.Application.Features.Activity.EventHandlers;
using Crm.Application.Features.Leads.Shared.Sheet;
using Crm.Domain.Leads;
using Crm.Domain.Leads.Enums;
using Crm.Domain.Leads.ValueObjects;

namespace UnitTests.Crm.Activity;

/// <summary>Lead actions of a person read as «Журнал дій» entries; the sheet sync is not listed (ACT-001).</summary>
public sealed class LeadActivityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 8, 0, 0, TimeSpan.Zero);
    private static readonly LeadPerson Anastasia = new(new UserId(17), "Анастасія");
    private readonly LeadActivity _describer = new();

    private static readonly string?[] Header =
    [
        "ID", "Кто взял в работу", "Предприятие", "Результат звонка",
    ];

    private static Lead NewLead() =>
        Lead.Create(LeadSheetParser.Parse([Header, ["1", null, "L&S Distributions", null]]).Value.Rows[0], Now);

    private static IReadOnlyCollection<IDomainEvent> Events(IHasDomainEvents source) => source.DomainEvents;

    [Test]
    public void Taking_and_releasing_are_listed()
    {
        var lead = NewLead();

        lead.Take(Anastasia, Now);
        lead.Release(Now);

        var entries = Events(lead).Select(_describer.Describe).ToList();
        Assert.That(entries.Select(x => x!.Summary), Is.EqualTo(new[]
        {
            "Взято в роботу",
            "Повернуто у вільні (був у Анастасія)",
        }));
        Assert.That(entries[0]!.SubjectTitle, Is.EqualTo("L&S Distributions"));
    }

    [Test]
    public void A_recorded_call_names_the_result()
    {
        var lead = NewLead();
        lead.Take(Anastasia, Now);
        ((IHasDomainEvents)lead).ClearDomainEvents();

        lead.RecordCall(Anastasia, LeadCallResult.Create("Не взяли", LeadOutcome.Retry, 1, 1), " Перезвонити ", null, Now);

        Assert.That(_describer.Describe(Events(lead).Single())!.Summary, Is.EqualTo("Дзвінок: Не взяли; Перезвонити"));
    }

    [Test]
    public void The_sheet_sync_raises_nothing()
    {
        var lead = NewLead();

        lead.ApplySheetOwner("Анастасія", Anastasia, Now);
        lead.ApplySheetCall("Не взяли", Anastasia, LeadCallResult.Create("Не взяли", LeadOutcome.Retry, 1, 1), null, null, Now);

        Assert.That(lead.OwnerUserId, Is.EqualTo(Anastasia.UserId));
        Assert.That(lead.CallCount, Is.EqualTo(1));
        Assert.That(Events(lead), Is.Empty);
    }

    [Test]
    public void Describing_a_category_lists_the_old_and_new_text()
    {
        var category = LeadCategory.Create("a", null);

        category.Describe("Великі підприємства");

        var entry = _describer.Describe(Events(category).Single());
        Assert.That(entry!.Changes, Is.EqualTo(new[] { new ActivityChangeItem("Опис", null, "Великі підприємства") }));
        Assert.That(entry.Action, Is.EqualTo(ActivityActions.Changed));
    }
}
