using BuildingBlocks.Domain.Identifiers.People;
using ActivityLog.Contracts.Activity;
using BuildingBlocks.Domain.Events;
using Crm.Application.Features.Activity.EventHandlers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Domain.Quotes.ValueObjects;

namespace UnitTests.Crm.Activity;

/// <summary>КП actions raise domain events that read as «Журнал дій» entries (ACT-001).</summary>
public sealed class QuoteActivityTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);
    private static readonly QuotePerson Manager = new(new UserId(5), "Юлія");
    private readonly QuoteActivity _describer = new();

    private static Quote NewQuote() =>
        Quote.Create(new QuoteNumber(2026, 11, 3), QuoteKind.Request, Manager,
            new QuoteHeader("Саша — 50 курток", null, "Олександр", "ТОВ «Агро»", null, null), [], Now).Value;

    private static IReadOnlyCollection<IDomainEvent> Events(IHasDomainEvents source) => source.DomainEvents;

    [Test]
    public void Creating_a_quote_is_listed_as_created_with_number_and_name()
    {
        var quote = NewQuote();

        var entry = _describer.Describe(Events(quote).Single());

        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Created));
        Assert.That(entry.SubjectType, Is.EqualTo(QuoteActivity.Subject));
        Assert.That(entry.SubjectTitle, Does.Contain("Саша — 50 курток"));
    }

    [Test]
    public void Saving_lists_only_the_fields_that_changed()
    {
        var quote = NewQuote();
        ((IHasDomainEvents)quote).ClearDomainEvents();

        quote.Update(new QuoteHeader("Саша — 60 курток", null, "Олександр", "ТОВ «Агро»", null, null), [], Now);
        quote.Record(QuoteEventKind.Saved, Manager, null, Now);

        var entry = _describer.Describe(Events(quote).Single());
        Assert.That(entry!.Action, Is.EqualTo(ActivityActions.Changed));
        Assert.That(entry.Changes, Is.EqualTo(new[] { new ActivityChangeItem("Назва", "Саша — 50 курток", "Саша — 60 курток") }));
    }

    [Test]
    public void Saving_without_changes_is_not_listed()
    {
        var quote = NewQuote();
        ((IHasDomainEvents)quote).ClearDomainEvents();

        quote.Update(new QuoteHeader("Саша — 50 курток", null, "Олександр", "ТОВ «Агро»", null, null), [], Now);
        quote.Record(QuoteEventKind.Saved, Manager, null, Now);

        Assert.That(_describer.Describe(Events(quote).Single()), Is.Null);
    }

    [Test]
    public void Deleting_a_quote_is_listed_as_deleted()
    {
        var quote = NewQuote();
        ((IHasDomainEvents)quote).ClearDomainEvents();

        quote.MarkDeleted();

        Assert.That(_describer.Describe(Events(quote).Single())!.Action, Is.EqualTo(ActivityActions.Deleted));
    }

    [Test]
    public void Events_of_other_modules_are_not_described() =>
        Assert.That(_describer.Describe(new object()), Is.Null);
}
