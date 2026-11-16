using BuildingBlocks.Application.Actors;
using BuildingBlocks.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Platform.DomainEvents;

/// <summary>ACT-001 R-01, R-04, ADR-0016: events become outbox messages in the same save.</summary>
public sealed class DomainEventOutboxInterceptorTests
{
    private static readonly Actor Manager = new(7, "manager@brevi.test", ["Manager"]);

    [Test]
    public async Task A_raised_event_is_saved_as_an_outbox_message_with_the_change()
    {
        using var harness = DomainEventTestDb.Create(Manager);
        await using (var db = harness.Open())
        {
            db.Notes.Add(Note.Create("КП №1"));
            await db.SaveChangesAsync();
        }

        await using var check = harness.Open();
        var message = await check.OutboxMessages.SingleAsync();
        Assert.That(message.Type, Is.EqualTo("test.note"));
        var note = await check.Notes.SingleAsync();
        Assert.That(message.Payload, Does.Contain("NoteCreated").And.Contain("\"userId\":7")
            .And.Contain($"\"noteId\":{note.Id}"), "the database id of the new row is in the message");
        Assert.That(note.Id, Is.GreaterThan(0));
        Assert.That(harness.Saved.Take(), Is.EqualTo(new[] { message.Id }));
    }

    [Test]
    public async Task An_import_that_suppresses_events_saves_its_rows_without_messages()
    {
        using var harness = DomainEventTestDb.Create(Manager);
        harness.Suppression.Suppress();
        await using (var db = harness.Open())
        {
            db.Notes.AddRange(Note.Create("row 1"), Note.Create("row 2"));
            await db.SaveChangesAsync();
        }

        await using var check = harness.Open();
        Assert.That(await check.Notes.CountAsync(), Is.EqualTo(2));
        Assert.That(await check.OutboxMessages.AnyAsync(), Is.False);
        Assert.That(harness.Saved.Take(), Is.Empty);
    }

    [Test]
    public async Task Events_are_cleared_after_the_save_so_a_second_save_writes_nothing_more()
    {
        using var harness = DomainEventTestDb.Create(Manager);
        await using var db = harness.Open();
        var note = Note.Create("A");
        db.Notes.Add(note);
        await db.SaveChangesAsync();
        Assert.That(((IHasDomainEvents)note).DomainEvents, Is.Empty);

        note.Rename("B");
        await db.SaveChangesAsync();
        await db.SaveChangesAsync();

        Assert.That(await db.OutboxMessages.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task A_failed_save_writes_no_message_and_remembers_no_id()
    {
        using var harness = DomainEventTestDb.Create(Manager);
        await using (var db = harness.Open())
        {
            db.Notes.Add(Note.Create("first"));
            await db.SaveChangesAsync();
        }

        await using (var db = harness.Open())
        {
            var first = await db.Notes.AsNoTracking().FirstAsync();
            harness.Saved.Take();
            var clash = Note.Create("clash");
            db.Entry(clash).Property(x => x.Id).CurrentValue = first.Id;
            db.Notes.Add(clash);
            await Assert.ThatAsync(() => db.SaveChangesAsync(), Throws.InstanceOf<DbUpdateException>());
        }

        await using var check = harness.Open();
        Assert.That(await check.OutboxMessages.CountAsync(), Is.EqualTo(1));
        Assert.That(harness.Saved.Take(), Is.Empty);
    }

    [Test]
    public async Task Without_a_signed_in_person_the_describer_writes_nothing()
    {
        using var harness = DomainEventTestDb.Create(null);
        await using var db = harness.Open();
        db.Notes.Add(Note.Create("import"));
        await db.SaveChangesAsync();

        Assert.That(await db.OutboxMessages.CountAsync(), Is.Zero);
        Assert.That(await db.Notes.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Renaming_to_the_same_title_raises_nothing()
    {
        using var harness = DomainEventTestDb.Create(Manager);
        await using var db = harness.Open();
        var note = Note.Create("A");
        db.Notes.Add(note);
        await db.SaveChangesAsync();

        note.Rename("A");
        await db.SaveChangesAsync();

        Assert.That(await db.OutboxMessages.CountAsync(), Is.EqualTo(1));
    }
}

/// <summary>The messages are written by a second save; when it fails the change rolls back with them.</summary>
public sealed class DomainEventOutboxRollbackTests
{
    [Test]
    public async Task A_failing_message_save_rolls_the_change_back()
    {
        using var harness = DomainEventTestDb.Create(new Actor(7, "m", ["Manager"]), failMessages: true);
        await using (var db = harness.Open())
        {
            db.Notes.Add(Note.Create("A"));
            await Assert.ThatAsync(() => db.SaveChangesAsync(), Throws.InvalidOperationException);
        }

        await using var check = harness.Open();
        Assert.That(await check.Notes.CountAsync(), Is.Zero);
        Assert.That(harness.Saved.Take(), Is.Empty);
    }

    [Test]
    public async Task Inside_the_callers_transaction_the_caller_decides()
    {
        using var harness = DomainEventTestDb.Create(new Actor(7, "m", ["Manager"]));
        await using (var db = harness.Open())
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            db.Notes.Add(Note.Create("A"));
            await db.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        await using var check = harness.Open();
        Assert.That(await check.Notes.CountAsync(), Is.Zero);
        Assert.That(await check.OutboxMessages.CountAsync(), Is.Zero);
    }

    [Test]
    public void The_sync_save_writes_the_message_too()
    {
        using var harness = DomainEventTestDb.Create(new Actor(7, "m", ["Manager"]));
        using var db = harness.Open();
        db.Notes.Add(Note.Create("A"));
        db.SaveChanges();
        Assert.That(db.OutboxMessages.Count(), Is.EqualTo(1));
    }
}
