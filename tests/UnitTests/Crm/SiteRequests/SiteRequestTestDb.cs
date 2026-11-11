using Crm.Application.SiteRequests.Abstractions;
using Crm.Application.Tasks.Staff;
using Crm.Domain.SiteRequests;
using Crm.Infrastructure.Persistence.Configurations.SiteRequests;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Crm.SiteRequests;

/// <summary>The site request table of the Crm module on in-memory SQLite, with the real mapping.</summary>
internal sealed class SiteRequestTestDb(DbContextOptions<SiteRequestTestDb> options) : DbContext(options), ISiteRequestStore
{
    public DbSet<SiteRequest> SiteRequests => Set<SiteRequest>();

    protected override void OnModelCreating(ModelBuilder builder) =>
        builder.ApplyConfiguration(new SiteRequestConfiguration());

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
