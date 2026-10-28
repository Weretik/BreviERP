using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.Files.Storage;

namespace Crm.Infrastructure.Files.Storage;

// Optional file configuration must not prevent unrelated custom calculations from resolving.
public sealed class ConfiguredPrivateFileStore(PrivateFileStorageOptions options, TimeProvider? clock = null) : IPrivateFileStore, IReservedPrivateFileStore
{
    private readonly Lazy<FileSystemPrivateFileStore> inner = new(() => new FileSystemPrivateFileStore(options, clock));

    public async Task<Result> PrepareAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try { return await inner.Value.PrepareAsync(ct); }
        catch (Exception ex) when (Unavailable(ex)) { return Result.Error("private_file_storage_unavailable"); }
    }

    public Task<Result<PrivateStoredFile>> WriteAsync(Guid root, Stream content, string type, CancellationToken ct)
        => GuardAsync(() => inner.Value.WriteAsync(root, content, type, ct), ct);

    public Task<Result<PrivateStoredFile>> WriteReservedAsync(Guid root, Guid fileId, Stream content, string type, DateTimeOffset expiresAt, CancellationToken ct)
        => GuardAsync(() => inner.Value.WriteReservedAsync(root, fileId, content, type, expiresAt, ct), ct);

    public Task<Result<PrivateFileContent>> ReadAsync(Guid root, Guid fileId, CancellationToken ct)
        => GuardAsync(() => inner.Value.ReadAsync(root, fileId, ct), ct);

    public async Task<Result> DeleteTemporaryAsync(Guid root, Guid fileId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try { return await inner.Value.DeleteTemporaryAsync(root, fileId, ct); }
        catch (Exception ex) when (Unavailable(ex)) { return Result.Error("private_file_storage_unavailable"); }
    }

    private static async Task<Result<T>> GuardAsync<T>(Func<Task<Result<T>>> action, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try { return await action(); }
        catch (Exception ex) when (Unavailable(ex)) { return Result.Error("private_file_storage_unavailable"); }
    }

    private static bool Unavailable(Exception error) => error is InvalidOperationException or IOException or UnauthorizedAccessException;
}
