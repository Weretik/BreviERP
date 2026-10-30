using System.Security.Cryptography;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.Files.Validation;
using Crm.Application.Files.Storage;

namespace Crm.Infrastructure.Files.Storage;

// Dedicated private volume adapter. No public URL, deletion or caller controlled filename is exposed.
public sealed class FileSystemPrivateFileStore : IPrivateFileStore, IReservedPrivateFileStore
{
    private readonly string root;
    private readonly TimeProvider time;
    public FileSystemPrivateFileStore(PrivateFileStorageOptions options, TimeProvider? clock = null)
    {
        root = PrivateStorageDirectory.Open(options);
        time = clock ?? TimeProvider.System;
    }

    public Task<Result> PrepareAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        PrivateStorageDirectory.EnsurePrivate(root);
        return Task.FromResult(Result.Success());
    }

    public async Task<Result<PrivateStoredFile>> WriteAsync(Guid configurationId, Stream content, string declaredContentType, CancellationToken cancellationToken)
        => await WriteReservedAsync(configurationId, Guid.NewGuid(), content, declaredContentType, DateTimeOffset.MaxValue, cancellationToken);

    public async Task<Result<PrivateStoredFile>> WriteReservedAsync(Guid configurationId, Guid fileId, Stream content,
        string declaredContentType, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        if (configurationId == Guid.Empty || fileId == Guid.Empty) return Result.Invalid(new ValidationError("configurationId", "Configuration is required."));
        var bytes = await ReadBoundedAsync(content, cancellationToken);
        if (bytes is null || !PrivateFilePolicy.MatchesAllowedContent(bytes, declaredContentType))
            return Result.Invalid(new ValidationError("file", "Expected actual PDF, JPEG or PNG content up to 20 MiB matching the declared MIME type."));
        PrivateStorageDirectory.EnsurePrivate(root);
        var scope = Path.Combine(root, configurationId.ToString("N"));
        PrivateStorageDirectory.RejectLinks(scope);
        Directory.CreateDirectory(scope);
        await using var quotaLock = await LockAsync(Path.Combine(scope, ".write-lock"), cancellationToken);
        if (time.GetUtcNow() >= expiresAt) return Result.Conflict("file_upload_expired");
        if (Directory.Exists(Path.Combine(scope, fileId.ToString("N"))))
        {
            var existing = await ReadAsync(configurationId, fileId, cancellationToken);
            if (!existing.IsSuccess) return Result.Error("Private file integrity check failed.");
            return existing.Value.File.Length == bytes.Length && existing.Value.File.ContentType == declaredContentType
                && existing.Value.File.Sha256 == Convert.ToHexString(SHA256.HashData(bytes))
                ? Result.Success(existing.Value.File) : Result.Conflict("reserved_file_payload_conflict");
        }
        if (Directory.EnumerateDirectories(scope).Count(x => Guid.TryParseExact(Path.GetFileName(x), "N", out _)) >= PrivateFilePolicy.MaximumFilesPerConfiguration)
            return Result.Invalid(new ValidationError("file", "A configuration permits at most 30 stored files."));
        var metadata = new PrivateStoredFile(fileId, configurationId, declaredContentType, bytes.Length, Convert.ToHexString(SHA256.HashData(bytes)));
        var pending = Path.Combine(scope, ".pending-" + fileId.ToString("N"));
        PrivateStorageDirectory.RejectLinks(pending);
        if (Directory.Exists(pending)) Directory.Delete(pending, true);
        Directory.CreateDirectory(pending);
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(pending, "content"), bytes, cancellationToken);
            await File.WriteAllTextAsync(Path.Combine(pending, "metadata.json"), JsonSerializer.Serialize(metadata), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (time.GetUtcNow() >= expiresAt) return Result.Conflict("file_upload_expired");
            Directory.Move(pending, Path.Combine(scope, metadata.FileId.ToString("N")));
            return Result.Success(metadata);
        }
        finally { if (Directory.Exists(pending)) Directory.Delete(pending, true); }
    }

    public async Task<Result> DeleteTemporaryAsync(Guid configurationId, Guid fileId, CancellationToken cancellationToken)
    {
        if (configurationId == Guid.Empty || fileId == Guid.Empty) return Result.Invalid(new ValidationError("fileId", "Invalid file identity."));
        PrivateStorageDirectory.EnsurePrivate(root);
        var scope = Path.Combine(root, configurationId.ToString("N"));
        PrivateStorageDirectory.RejectLinks(scope);
        if (!Directory.Exists(scope)) return Result.Success();
        await using var quotaLock = await LockAsync(Path.Combine(scope, ".write-lock"), cancellationToken);
        foreach (var name in new[] { fileId.ToString("N"), ".pending-" + fileId.ToString("N") })
        {
            var path = Path.Combine(scope, name);
            PrivateStorageDirectory.RejectLinks(path);
            if (!Directory.Exists(path)) continue;
            PrivateStorageDirectory.RejectLinks(Path.Combine(path, "content"));
            PrivateStorageDirectory.RejectLinks(Path.Combine(path, "metadata.json"));
            cancellationToken.ThrowIfCancellationRequested();
            Directory.Delete(path, true);
        }
        return Result.Success();
    }

    public async Task<Result<PrivateFileContent>> ReadAsync(Guid configurationId, Guid fileId, CancellationToken cancellationToken)
    {
        if (configurationId == Guid.Empty || fileId == Guid.Empty) return Result.NotFound();
        PrivateStorageDirectory.EnsurePrivate(root);
        var path = Path.Combine(root, configurationId.ToString("N"), fileId.ToString("N"));
        PrivateStorageDirectory.RejectLinks(path);
        if (!Directory.Exists(path)) return Result.NotFound();
        var metadataPath = Path.Combine(path, "metadata.json");
        var contentPath = Path.Combine(path, "content");
        PrivateStorageDirectory.RejectLinks(metadataPath);
        PrivateStorageDirectory.RejectLinks(contentPath);
        if (!File.Exists(metadataPath) || !File.Exists(contentPath) || new FileInfo(metadataPath).Length > 4096) return Result.Error("Private file integrity check failed.");
        PrivateStoredFile? metadata;
        try { metadata = JsonSerializer.Deserialize<PrivateStoredFile>(await File.ReadAllTextAsync(metadataPath, cancellationToken)); }
        catch (JsonException) { return Result.Error("Private file integrity check failed."); }
        await using var stream = new FileStream(contentPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var bytes = await ReadBoundedAsync(stream, cancellationToken);
        if (metadata is null || metadata.FileId != fileId || metadata.ConfigurationId != configurationId || bytes is null || metadata.Length != bytes.Length || !PrivateFilePolicy.MatchesAllowedContent(bytes, metadata.ContentType) || metadata.Sha256 != Convert.ToHexString(SHA256.HashData(bytes)))
            return Result.Error("Private file integrity check failed.");
        return Result.Success(new PrivateFileContent(metadata, bytes));
    }

    private static async Task<byte[]?> ReadBoundedAsync(Stream content, CancellationToken cancellationToken)
    {
        using var destination = new MemoryStream();
        var buffer = new byte[81920];
        while (true)
        {
            var read = await content.ReadAsync(buffer, cancellationToken);
            if (read == 0) break;
            if (destination.Length + read > PrivateFilePolicy.MaximumBytes) return null;
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
        return destination.ToArray();
    }

    private static async Task<FileStream> LockAsync(string path, CancellationToken cancellationToken)
    {
        PrivateStorageDirectory.RejectLinks(path);
        for (var attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try { return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException) when (attempt < 100) { await Task.Delay(50, cancellationToken); }
        }
    }
}
