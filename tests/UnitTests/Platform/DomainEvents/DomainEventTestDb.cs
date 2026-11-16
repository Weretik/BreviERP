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

internal sealed record NoteCreated(Guid NoteId, string Title) : IDomainEvent;

internal sealed record NoteRenamed(Guid NoteId, string From, string To) : IDomainEvent;

/// <summary>A tiny aggregate that raises events like the real ones do.</summary>
internal sealed class Note : DomainEventSource
{
    private Note() { }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;

    public static Note Create(string title)
    {
        var note = new Note { Id = Guid.CreateVersion7(), Title = title };
        note.Raise(new NoteCreated(note.Id, title));
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
            : [OutboxMessage.Create("test.note", new { Event = domainEvent.GetType().Name, actor.UserId }, now)];
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

    public static Harness Create(Actor? actor)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var saved = new SavedOutboxMessages();
        var currentActor = new FixedActor(actor);
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 16, 10, 0, 0, TimeSpan.Zero));
        var interceptor = new DomainEventOutboxInterceptor([new NoteOutbox()], currentActor, saved, clock);
        var options = new DbContextOptionsBuilder<DomainEventTestDb>().UseSqlite(connection)
            .AddInterceptors(interceptor).Options;
        using (var db = new DomainEventTestDb(options)) db.Database.EnsureCreated();
        return new Harness(connection, () => new DomainEventTestDb(options), saved, currentActor);
    }

    internal sealed record Harness(SqliteConnection Connection, Func<DomainEventTestDb> Open,
        SavedOutboxMessages Saved, FixedActor Actor) : IDisposable
    {
        public void Dispose() => Connection.Dispose();
    }
}
