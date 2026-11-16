using BuildingBlocks.Application.Actors;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Infrastructure.DomainEvents;
using BuildingBlocks.Infrastructure.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace UnitTests.Platform.DomainEvents;

/// <summary>Carries the note: its database id is known only after the insert.</summary>
internal sealed record NoteCreated(Note Note) : IDomainEvent;

internal sealed record NoteRenamed(int NoteId, string From, string To) : IDomainEvent;

/// <summary>A tiny aggregate that raises events like the real ones do.</summary>
internal sealed class Note : DomainEventSource
{
    private Note() { }

    public int Id { get; private set; }
    public string Title { get; private set; } = null!;

    public static Note Create(string title)
    {
        var note = new Note { Title = title };
        note.Raise(new NoteCreated(note));
        return note;
    }

    public void Rename(string title)
    {
        if (title == Title) return;
        Raise(new NoteRenamed(Id, Title, title));
        Title = title;
    }
}

/// <summary>Writes one outbox message per note event, with who did it, like a module's activity describer.</summary>
internal sealed class NoteOutbox : IDomainEventOutbox
{
    public IReadOnlyList<OutboxMessage> Write(IDomainEvent domainEvent, Actor? actor, DateTimeOffset now) =>
        actor is null || domainEvent is not (NoteCreated or NoteRenamed)
            ? []
            : [OutboxMessage.Create("test.note", new
            {
                Event = domainEvent.GetType().Name,
                actor.UserId,
                NoteId = domainEvent is NoteCreated created ? created.Note.Id : ((NoteRenamed)domainEvent).NoteId,
            }, now)];
}

/// <summary>Fails while the messages are written, after the change itself was saved.</summary>
internal sealed class BrokenOutbox : IDomainEventOutbox
{
    public IReadOnlyList<OutboxMessage> Write(IDomainEvent domainEvent, Actor? actor, DateTimeOffset now) =>
        throw new InvalidOperationException("describer failed");
}

internal sealed class FixedActor(Actor? actor) : ICurrentActor
{
    public Actor? Current { get; set; } = actor;
}

internal sealed class DomainEventTestDb(DbContextOptions<DomainEventTestDb> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Note>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).HasMaxLength(20);
        });
        builder.AddOutbox();
    }

    public static Harness Create(Actor? actor, bool failMessages = false)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var saved = new SavedOutboxMessages();
        var currentActor = new FixedActor(actor);
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 16, 10, 0, 0, TimeSpan.Zero));
        IDomainEventOutbox outbox = failMessages ? new BrokenOutbox() : new NoteOutbox();
        var suppression = new DomainEventSuppression();
        var interceptor = new DomainEventOutboxInterceptor([outbox], currentActor, saved, suppression, clock);
        var options = new DbContextOptionsBuilder<DomainEventTestDb>().UseSqlite(connection)
            .AddInterceptors(interceptor).Options;
        using (var db = new DomainEventTestDb(options)) db.Database.EnsureCreated();
        return new Harness(connection, () => new DomainEventTestDb(options), saved, currentActor, suppression);
    }

    internal sealed record Harness(SqliteConnection Connection, Func<DomainEventTestDb> Open,
        SavedOutboxMessages Saved, FixedActor Actor, DomainEventSuppression Suppression) : IDisposable
    {
        public void Dispose() => Connection.Dispose();
    }
}
