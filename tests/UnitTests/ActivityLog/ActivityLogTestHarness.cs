using ActivityLog.Application.Abstractions;
using ActivityLog.Contracts.Activity;
using ActivityLog.Domain.Entries;
using ActivityLog.Domain.Filters;
using ActivityLog.Infrastructure.Persistence.Configurations;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UnitTests.TestSupport;
using Microsoft.Extensions.Time.Testing;

namespace UnitTests.ActivityLog;

/// <summary>The «Журнал дій» table on in-memory SQLite with the real mapping.</summary>
internal sealed class ActivityTestDb(DbContextOptions<ActivityTestDb> options) : DbContext(options), IActivityLogDbContext
{
    public DbSet<ActivityEntry> Entries => Set<ActivityEntry>();
    public DbSet<ActivityActor> Actors => Set<ActivityActor>();
    public DbSet<ActivitySubjectType> SubjectTypes => Set<ActivitySubjectType>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.AddTypedIds(typeof(ActivityEntry).Assembly);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new ActivityEntryConfiguration());
        builder.ApplyConfiguration(new ActivityActorConfiguration());
        builder.ApplyConfiguration(new ActivitySubjectTypeConfiguration());
    }
}

internal sealed class FakeActorNames : IActorNames
{
    public Dictionary<int, string> Names { get; } = new() { [7] = "Олена Менеджер" };

    public Task<string?> FindAsync(int userId, CancellationToken cancellationToken) =>
        Task.FromResult(Names.TryGetValue(userId, out var name) ? name : null);
}

internal sealed class ActivityLogTestHarness : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ActivityTestDb> _options;

    public ActivityLogTestHarness()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<ActivityTestDb>().UseSqlite(_connection)
            .ReplaceService<IModelCustomizer, SqliteInstantsCustomizer>().Options;
        using var db = Open();
        db.Database.EnsureCreated();
    }

    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 11, 16, 10, 0, 0, TimeSpan.Zero));
    public FakeActorNames Names { get; } = new();

    public ActivityTestDb Open() => new(_options);

    public static ActivityRecorded Activity(
        int actorId = 7,
        string[]? roles = null,
        string action = ActivityActions.Created,
        string subjectType = "quote",
        string subjectTitle = "КП №125",
        DateTimeOffset? at = null,
        Guid? id = null,
        IReadOnlyList<ActivityChangeItem>? changes = null) =>
        new(id ?? Guid.CreateVersion7(), at ?? new DateTimeOffset(2026, 11, 16, 9, 0, 0, TimeSpan.Zero), actorId,
            $"user{actorId}@brevi.test", roles ?? ["Manager"], action, subjectType,
            subjectType == "quote" ? "КП" : "Виробниче завдання", Guid.NewGuid().ToString(), subjectTitle, null,
            changes ?? [], "Crm");

    public static string Payload(ActivityRecorded activity) =>
        OutboxMessage.Create(ActivityMessage.Type, activity, DateTimeOffset.UnixEpoch).Payload;

    public void Dispose() => _connection.Dispose();
}
