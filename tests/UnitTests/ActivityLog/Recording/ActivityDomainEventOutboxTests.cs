using ActivityLog.Application.Features.Entries.EventHandlers;
using ActivityLog.Contracts.Activity;
using BuildingBlocks.Application.Actors;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Domain.Events;

namespace UnitTests.ActivityLog.Recording;

/// <summary>ACT-001 R-01, R-02, R-03, ADR-0016: a module's description + the actor = one activity message.</summary>
public sealed class ActivityDomainEventOutboxTests
{
    private sealed record Renamed(int Id, string From, string To) : IDomainEvent;

    private sealed class Describer : IActivityDescriber
    {
        public ActivityDescription? Describe(object domainEvent) => domainEvent is Renamed renamed
            ? new ActivityDescription(ActivityActions.Changed, "quote", "КП", renamed.Id.ToString(), "№" + renamed.Id, null,
                new ActivityChanges().Add("Назва", renamed.From, renamed.To).Items, "Crm")
            : null;
    }

    private static readonly DateTimeOffset Now = new(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);

    [Test]
    public void A_described_event_becomes_one_message_with_the_actor()
    {
        var messages = new ActivityDomainEventOutbox([new Describer()])
            .Write(new Renamed(5, "A", "B"), new Actor(7, "m@brevi.test", ["Manager"]), Now);

        var message = messages.Single();
        Assert.That(message.Type, Is.EqualTo(ActivityMessage.Type));
        var activity = OutboxMessage.Read<ActivityRecorded>(message.Payload);
        Assert.That(activity.ActorId, Is.EqualTo(7));
        Assert.That(activity.ActorLogin, Is.EqualTo("m@brevi.test"));
        Assert.That(activity.ActorRoles, Is.EqualTo(new[] { "Manager" }));
        Assert.That(activity.OccurredAt, Is.EqualTo(Now));
        Assert.That(activity.EventId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(activity.Changes.Single(), Is.EqualTo(new ActivityChangeItem("Назва", "A", "B")));
    }

    [Test]
    public void Without_a_person_or_a_description_nothing_is_written()
    {
        var outbox = new ActivityDomainEventOutbox([new Describer()]);
        Assert.That(outbox.Write(new Renamed(5, "A", "B"), null, Now), Is.Empty);
        Assert.That(outbox.Write(new UnknownEvent(), new Actor(7, "m", []), Now), Is.Empty);
    }

    [Test]
    public void Changes_list_only_fields_that_differ_in_display_form()
    {
        var changes = new ActivityChanges()
            .Add("Назва", "A", " A ")
            .Add("Сума", 1200m, 1500.5m, "грн")
            .Add("Дата", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 2))
            .Add("Примітка", null, "")
            .Add("Активний", true, false);
        Assert.That(changes.Items, Is.EqualTo(new[]
        {
            new ActivityChangeItem("Сума", "1 200 грн", "1 500,50 грн"),
            new ActivityChangeItem("Дата", "01.11.2026", "02.11.2026"),
            new ActivityChangeItem("Активний", "так", "ні"),
        }));
    }

    private sealed record UnknownEvent : IDomainEvent;
}
