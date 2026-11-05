using Accounting.Application.Abstractions;

namespace UnitTests.Accounting;

/// <summary>The private file storage in memory.</summary>
internal sealed class FakeAccountingFileStorage(bool configured = true) : IAccountingFileStorage
{
    public Dictionary<string, (byte[] Bytes, string ContentType)> Files { get; } = [];

    public bool IsConfigured => configured;

    /// <summary>The bucket does not answer.</summary>
    public bool Down { get; set; }

    public async Task UploadAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken)
    {
        if (Down) throw new AccountingFileStorageUnavailableException("down");
        using var copy = new MemoryStream();
        await content.CopyToAsync(copy, cancellationToken);
        Files[storageKey] = (copy.ToArray(), contentType);
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken) =>
        Task.FromResult<Stream?>(Files.TryGetValue(storageKey, out var file) ? new MemoryStream(file.Bytes) : null);

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        Files.Remove(storageKey);
        return Task.CompletedTask;
    }
}
