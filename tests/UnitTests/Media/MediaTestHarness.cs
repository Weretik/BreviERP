using Media.Application.Abstractions;
using Media.Application.Contracts;
using Media.Application.Files.UploadFile;
using Media.Application.Folders.CreateFolder;
using Media.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace UnitTests.Media;

internal sealed class FakeMediaStorage : IMediaStorage
{
    public Dictionary<string, string> Files { get; } = [];
    public List<string> Deleted { get; } = [];
    public string ProviderName => "fake";

    public Task<string> UploadAsync(Stream content, string storageKey, string contentType, CancellationToken cancellationToken)
    {
        var url = $"https://cdn.example.test{storageKey}";
        Files[storageKey] = url;
        return Task.FromResult(url);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        Deleted.Add(storageKey);
        Files.Remove(storageKey);
        return Task.CompletedTask;
    }
}

internal sealed class FakeMediaUsageReader : IMediaUsageReader
{
    public List<MediaUsage> Usages { get; } = [];

    public Task<IReadOnlyList<MediaUsage>> FindAsync(IReadOnlyCollection<int> fileIds, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<MediaUsage>>(Usages.Where(x => fileIds.Contains(x.FileId)).ToList());
}

/// <summary>Media module on SQLite in memory with a fake CDN and a fake list of product usages.</summary>
internal sealed class MediaTestHarness : IDisposable
{
    private readonly SqliteConnection _connection;

    public MediaTestHarness()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Db = new MediaDbContext(new DbContextOptionsBuilder<MediaDbContext>().UseSqlite(_connection).Options);
        Db.Database.EnsureCreated();
    }

    public MediaDbContext Db { get; }
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 3, 10, 0, 0, TimeSpan.Zero));
    public FakeMediaStorage Storage { get; } = new();
    public FakeMediaUsageReader Usages { get; } = new();
    public MediaUploadSettings Settings { get; } = new() { MaxFileSizeBytes = 1024 * 1024, BaseFolder = "/library/original" };

    public static byte[] Jpeg { get; } = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, 0x4A, 0x46, 0x49, 0x46, 0, 1];

    public async Task<int> FolderAsync(string name, int? parentId = null)
    {
        var result = await new CreateMediaFolderCommandHandler(Db, Time)
            .Handle(new CreateMediaFolderCommand(new MediaFolderInput(name, parentId)), CancellationToken.None);
        return result.Value.Id;
    }

    public async Task<MediaFileResponse> UploadAsync(string fileName, int? folderId)
    {
        await using var content = new MemoryStream(Jpeg);
        var result = await new UploadMediaFileCommandHandler(Db, Storage, Settings, Time, NullLogger<UploadMediaFileCommandHandler>.Instance)
            .Handle(new UploadMediaFileCommand(content, fileName, "image/jpeg", Jpeg.Length, folderId), CancellationToken.None);
        return result.Value;
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
