using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox;
using Crm.Domain.SiteRequests;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Crm.Application.Features.SiteRequests.Shared.Notify;
using Crm.Application.Abstractions.SiteRequests;
using Crm.Application.Abstractions.Tasks;
using Crm.Infrastructure.Persistence.Configurations;

namespace UnitTests.Crm.SiteRequests;

/// <summary>The site request table of the Crm module on in-memory SQLite, with the real mapping.</summary>
internal sealed class SiteRequestTestDb(DbContextOptions<SiteRequestTestDb> options) : DbContext(options), ISiteRequestStore
{
    public DbSet<SiteRequest> SiteRequests => Set<SiteRequest>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new SiteRequestConfiguration());
        builder.AddOutbox();
    }

    public static (SqliteConnection Connection, Func<SiteRequestTestDb> Open) Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<SiteRequestTestDb>().UseSqlite(connection).Options;
        using (var db = new SiteRequestTestDb(options)) db.Database.EnsureCreated();
        return (connection, () => new SiteRequestTestDb(options));
    }
}

internal sealed class FakeSiteRequestFiles : ISiteRequestFileStorage
{
    public List<(string Key, string ContentType, long Length)> Uploads { get; } = [];

    public Task<string> UploadAsync(Stream content, string storageKey, string contentType,
        CancellationToken cancellationToken)
    {
        Uploads.Add((storageKey, contentType, content.Length));
        return Task.FromResult($"https://cdn.test{storageKey}");
    }
}

internal sealed class FakeSiteRequestNotifier : ISiteRequestNotifier
{
    public List<string> Messages { get; } = [];
    public string? Error { get; set; }
    public bool Throws { get; set; }

    public Task<string?> NotifyAsync(string html, CancellationToken cancellationToken)
    {
        if (Throws) throw new HttpRequestException("down");
        Messages.Add(html);
        return Task.FromResult(Error);
    }
}

internal sealed class FakeSiteRequestStaff : ITaskStaffReader
{
    public Task<IReadOnlyList<TaskStaffMember>> ListAssigneesAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<TaskStaffMember>>([]);

    public Task<TaskStaffMember?> FindAssigneeAsync(int userId, CancellationToken cancellationToken) =>
        FindAsync(userId, cancellationToken);

    public Task<TaskStaffMember?> FindAsync(int userId, CancellationToken cancellationToken) =>
        Task.FromResult<TaskStaffMember?>(userId is 1 or 2 or 3 ? new TaskStaffMember(userId, $"Менеджер {userId}", null) : null);
}

/// <summary>
/// Runs the Crm outbox messages at once on the same database, as the real dispatcher does after the save
/// (PLT-003 R-02); the PostgreSQL claim itself is tested in <c>OutboxDispatcherTests</c>.
/// </summary>
internal sealed class InlineSiteRequestOutbox(Func<SiteRequestTestDb> open, ISiteRequestNotifier notifier,
    SiteRequestLinks links, TimeProvider time) : IOutboxDispatcher
{
    public async Task DispatchAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        await using var db = open();
        foreach (var message in db.OutboxMessages.Where(x => ids.Contains(x.Id) && x.ProcessedAt == null).ToList())
        {
            await using var handlerDb = open();
            var error = await new NotifySiteRequestOutboxHandler(handlerDb, notifier, links, time,
                NullLogger<NotifySiteRequestOutboxHandler>.Instance).HandleAsync(message.Payload, cancellationToken);
            if (error is null) message.Succeed(time.GetUtcNow());
            else message.Fail(error, time.GetUtcNow());
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<OutboxRun> DispatchDueAsync(CancellationToken cancellationToken)
    {
        await using var db = open();
        var ids = db.OutboxMessages.Where(x => x.ProcessedAt == null).Select(x => x.Id).ToList();
        await DispatchAsync(ids, cancellationToken);
        return new OutboxRun(ids.Count, 0);
    }
}
