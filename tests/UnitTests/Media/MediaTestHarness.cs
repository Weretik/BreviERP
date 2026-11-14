using Media.Application.Abstractions;
using Media.Infrastructure.Images;
using Media.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Media.Application.Features.Files.Commands.UploadMediaFile;
using Media.Application.Features.Files.Shared;
using Media.Application.Features.Files.Shared.Images;
using Media.Application.Features.Folders.Commands.CreateMediaFolder;
using Media.Application.Features.Folders.Shared;

namespace UnitTests.Media;

internal sealed class FakeMediaStorage : IMediaStorage
{
    public Dictionary<string, string> Files { get; } = [];
    public Dictionary<string, byte[]> Contents { get; } = [];
    public List<string> Deleted { get; } = [];
    public string ProviderName => "fake";

    /// <summary>Uploads whose key starts with this prefix fail, like a CDN that is down.</summary>
    public string? FailUploadsUnder { get; set; }

    public async Task<string> UploadAsync(Stream content, string storageKey, string contentType, CancellationToken cancellationToken)
    {
        if (FailUploadsUnder is not null && storageKey.StartsWith(FailUploadsUnder, StringComparison.Ordinal))
            throw new HttpRequestException("CDN is down");
        var url = $"https://cdn.example.test{storageKey}";
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        Files[storageKey] = url;
        Contents[url] = buffer.ToArray();
        return url;
    }

    public Task<byte[]> DownloadAsync(string publicUrl, CancellationToken cancellationToken) =>
        Contents.TryGetValue(publicUrl, out var content)
            ? Task.FromResult(content)
            : throw new HttpRequestException($"404 {publicUrl}");

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
    public MediaUploadSettings Settings { get; } = new()
    {
        MaxFileSizeBytes = 1024 * 1024, BaseFolder = "/library/original", ThumbnailFolder = "/library/thumbs", ThumbnailMaxSize = 480,
    };

    public MediaImagePreparer Images => new(new SkiaMediaImageProcessor(), Storage, Settings);

    public UploadMediaFileHandler UploadHandler() =>
        new(Db, Storage, Images, Settings, Time, NullLogger<UploadMediaFileHandler>.Instance);

    public static byte[] Jpeg { get; } = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, 0x4A, 0x46, 0x49, 0x46, 0, 1];

    public async Task<int> FolderAsync(string name, int? parentId = null)
    {
        var result = await new CreateMediaFolderHandler(Db, Time)
            .Handle(new CreateMediaFolderCommand(new MediaFolderInput(name, parentId)), CancellationToken.None);
        return result.Value.Id;
    }

    public async Task<MediaFileResponse> UploadAsync(string fileName, int? folderId, byte[]? bytes = null, string contentType = "image/jpeg")
    {
        bytes ??= Jpeg;
        await using var content = new MemoryStream(bytes);
        var result = await UploadHandler()
            .Handle(new UploadMediaFileCommand(content, fileName, contentType, bytes.Length, folderId), CancellationToken.None);
        return result.Value;
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
