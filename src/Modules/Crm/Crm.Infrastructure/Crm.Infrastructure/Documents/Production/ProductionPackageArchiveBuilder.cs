using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Abstractions.Files;
using Crm.Application.CustomSewing.Production.Contracts;
using Crm.Application.CustomSewing.Production.Export;
using Crm.Application.CustomSewing.Production.Projection;
using Crm.Application.Files.Validation;

namespace Crm.Infrastructure.Documents.Production;

/// <summary>Deterministic private ZIP from an explicit manifest and verified pinned bytes.</summary>
public sealed class ProductionPackageArchiveBuilder(IPrivateFileStore files) : IProductionPackageArchiveBuilder
{
    public const int MaximumPackageBytes = 64 * 1024 * 1024;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly DateTimeOffset ZipTime = new(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public async Task<Result<byte[]>> BuildAsync(ProductionPackageManifest manifest,
        IReadOnlyList<ProductionCustomVersionSource> sources, CancellationToken ct)
    {
        var manifestBytes = JsonSerializer.SerializeToUtf8Bytes(manifest, Json);
        if (manifestBytes.Length > 1024 * 1024 || manifest.Files.Count > 30 ||
            manifest.Files.Sum(file => file.Length) > MaximumPackageBytes - manifestBytes.Length ||
            manifest.Files.Select(file => file.FilePublicId).Distinct().Count() != manifest.Files.Count)
            return Invalid("production_package_too_large");
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            await AddAsync(zip, "manifest.json", manifestBytes, ct);
            foreach (var file in manifest.Files.OrderBy(file => file.FilePublicId))
            {
                var extension = file.ContentType switch { "application/pdf" => "pdf", "image/png" => "png", "image/jpeg" => "jpg", _ => null };
                if (extension is null || file.ArchivePath != $"files/{file.FilePublicId:D}.{extension}" ||
                    file.Length is <= 0 or > PrivateFilePolicy.MaximumBytes)
                    return Invalid("production_file_metadata_invalid");
                var origins = sources.Where(source => source.Files.Any(pinned =>
                    pinned.Production && pinned.FilePublicId == file.FilePublicId &&
                    pinned.ContentType == file.ContentType && pinned.Length == file.Length &&
                    string.Equals(pinned.Sha256, file.Sha256, StringComparison.OrdinalIgnoreCase)))
                    .Select(source => source.Snapshot.DraftPublicId).Distinct().ToArray();
                if (origins.Length != 1) return Invalid("production_file_provenance_conflict");
                var read = await files.ReadAsync(origins[0], file.FilePublicId, ct);
                if (!read.IsSuccess) return Invalid("production_file_missing_or_inaccessible");
                var content = read.Value;
                if (content.File.ConfigurationId != origins[0] || content.File.FileId != file.FilePublicId ||
                    content.File.ContentType != file.ContentType || content.File.Length != file.Length ||
                    content.Bytes.Length != file.Length ||
                    !string.Equals(content.File.Sha256, file.Sha256, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(Convert.ToHexString(SHA256.HashData(content.Bytes.Span)), file.Sha256, StringComparison.OrdinalIgnoreCase) ||
                    !PrivateFilePolicy.MatchesAllowedContent(content.Bytes.Span, file.ContentType))
                    return Invalid("production_file_integrity_failed");
                await AddAsync(zip, file.ArchivePath, content.Bytes, ct);
            }
        }
        if (output.Length > MaximumPackageBytes) return Invalid("production_package_too_large");
        return Result.Success(output.ToArray());
    }

    private static async Task AddAsync(ZipArchive zip, string name, ReadOnlyMemory<byte> bytes, CancellationToken ct)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.NoCompression);
        entry.LastWriteTime = ZipTime;
        await using var stream = entry.Open();
        await stream.WriteAsync(bytes, ct);
    }
    private static Result<byte[]> Invalid(string code) => Result<byte[]>.Invalid(new ValidationError("productionFiles", code));
}
