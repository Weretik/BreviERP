using ActivityLog.Application.Features.Entries.EventHandlers;
using ActivityLog.Contracts.Activity;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.ActivityLog.Recording;

/// <summary>ACT-001 R-05, R-06, SC-06, SC-07, SC-11.</summary>
public sealed class RecordActivityOutboxHandlerTests
{
    [Test]
    public async Task An_activity_is_stored_with_the_staff_members_full_name()
    {
        using var harness = new ActivityLogTestHarness();
        var activity = ActivityLogTestHarness.Activity(changes: [new ActivityChangeItem("Сума", "1 200,00 грн", "1 500,00 грн")]);

        await using (var db = harness.Open())
        {
            var error = await new RecordActivityOutboxHandler(db, harness.Names, harness.Clock)
                .HandleAsync(ActivityLogTestHarness.Payload(activity), CancellationToken.None);
            Assert.That(error, Is.Null);
        }

        await using var check = harness.Open();
        var entry = await check.Entries.SingleAsync();
        Assert.That(entry.Id.Value, Is.EqualTo(activity.EventId));
        Assert.That(entry.ActorName, Is.EqualTo("Олена Менеджер"));
        Assert.That(entry.RoleNames, Is.EqualTo(new[] { "Manager" }));
        Assert.That(entry.SubjectTitle, Is.EqualTo("КП №125"));
        Assert.That(entry.Changes.Single().To, Is.EqualTo("1 500,00 грн"));
        Assert.That(entry.RecordedAt, Is.EqualTo(harness.Clock.GetUtcNow()));
    }

    [Test]
    public async Task The_same_message_twice_stores_one_entry()
    {
        using var harness = new ActivityLogTestHarness();
        var payload = ActivityLogTestHarness.Payload(ActivityLogTestHarness.Activity());

        for (var i = 0; i < 2; i++)
        {
            await using var db = harness.Open();
            Assert.That(await new RecordActivityOutboxHandler(db, harness.Names, harness.Clock)
                .HandleAsync(payload, CancellationToken.None), Is.Null);
        }

        await using var check = harness.Open();
        Assert.That(await check.Entries.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Without_a_full_name_the_login_is_kept()
    {
        using var harness = new ActivityLogTestHarness();
        await using (var db = harness.Open())
            await new RecordActivityOutboxHandler(db, harness.Names, harness.Clock)
                .HandleAsync(ActivityLogTestHarness.Payload(ActivityLogTestHarness.Activity(actorId: 9)), CancellationToken.None);

        await using var check = harness.Open();
        Assert.That((await check.Entries.SingleAsync()).ActorName, Is.EqualTo("user9@brevi.test"));
    }
}
