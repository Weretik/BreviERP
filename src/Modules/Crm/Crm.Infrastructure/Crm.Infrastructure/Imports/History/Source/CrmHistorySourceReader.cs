using System.Security;
using System.Text.Json;
using Crm.Application.Imports.History.Contracts;
using Crm.Infrastructure.Imports.History.Staging;

namespace Crm.Infrastructure.Imports.History.Source;

/// <summary>Verifies a frozen local source and returns metadata only; never resolves a database.</summary>
public sealed class CrmHistorySourceReader
{
    public async Task<CrmHistoryDryRunRequest> ReadAsync(string freezePath, string sourceRoot, CancellationToken ct = default)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            var root = CrmHistorySourceFiles.SourceRoot(sourceRoot);
            var control = await CrmHistorySourceFiles.ReadAsync(CrmHistorySourceFiles.Resolve(root, freezePath, explicitControl: true), ct);
            var frozen = CrmHistorySourceFiles.ReadStrict<CrmHistoryFrozenSource>(control);
            if (frozen.SchemaVersion != 1 || !CrmHistoryRowNormalizer.IsSourceId(frozen.DatasetId) || frozen.Blobs is null || frozen.Blobs.Count > CrmHistoryLimits.MaximumRows)
                throw CrmHistorySourceFiles.Invalid();
            var database = await CrmHistorySourceFiles.ReadAsync(CrmHistorySourceFiles.Resolve(root, frozen.DatabaseFile), ct);
            CrmHistorySourceFiles.VerifyHash(database, frozen.DatabaseSha256);
            var manifest = await CrmHistorySourceFiles.ReadAsync(CrmHistorySourceFiles.Resolve(root, frozen.ManifestFile), ct);
            CrmHistorySourceFiles.VerifyHash(manifest, frozen.ManifestSha256);
            var options = new CrmHistoryDryRunOptions(new Dictionary<string, int>(), new Dictionary<string, string>(), [], null, null);
            string? optionsHash = null;
            if ((frozen.OptionsFile is null) != (frozen.OptionsSha256 is null)) throw CrmHistorySourceFiles.Invalid();
            if (frozen.OptionsFile is not null)
            {
                var optionsBytes = await CrmHistorySourceFiles.ReadAsync(CrmHistorySourceFiles.Resolve(root, frozen.OptionsFile), ct);
                CrmHistorySourceFiles.VerifyHash(optionsBytes, frozen.OptionsSha256!);
                options = CrmHistorySourceFiles.ReadStrict<CrmHistoryDryRunOptions>(optionsBytes);
                if (options.OwnerMappings is null || options.ExactIdentityMappings is null || options.PreviousStaging is null)
                    throw CrmHistorySourceFiles.Invalid();
                optionsHash = CrmHistorySourceFiles.Hash(optionsBytes);
            }
            using var databaseDocument = JsonDocument.Parse(database);
            using var manifestDocument = JsonDocument.Parse(manifest);
            var proofs = await ReadBlobProofsAsync(root, frozen.Blobs, ct);
            var snapshot = CrmHistoryNativeSnapshotAdapter.Read(frozen.DatasetId, CrmHistorySourceFiles.Hash(database),
                CrmHistorySourceFiles.Hash(manifest), databaseDocument.RootElement, manifestDocument.RootElement, proofs);
            return new CrmHistoryDryRunRequest(snapshot with { ControlSha256 = CrmHistorySourceFiles.Hash(control), OptionsSha256 = optionsHash }, options);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or JsonException or NotSupportedException or SecurityException or InvalidOperationException or OverflowException)
        {
            // The original exception can contain private values, JSON fragments or local paths.
            throw CrmHistorySourceFiles.Invalid();
        }
    }

    private static async Task<IReadOnlyDictionary<string, CrmHistoryBlobProof>> ReadBlobProofsAsync(string root, IReadOnlyList<CrmHistoryFrozenBlob> blobs, CancellationToken ct)
    {
        var proofs = new Dictionary<string, CrmHistoryBlobProof>(StringComparer.Ordinal);
        foreach (var blob in blobs)
        {
            if (blob is null || !CrmHistoryRowNormalizer.IsSourceId(blob.SourceFileId) || !CrmHistorySourceFiles.IsHash(blob.Sha256)
                || blob.Length is <= 0 or > CrmHistorySourceFiles.MaximumBytes || proofs.ContainsKey(blob.SourceFileId))
                throw CrmHistorySourceFiles.Invalid();
            var bytes = await CrmHistorySourceFiles.ReadAsync(CrmHistorySourceFiles.Resolve(root, blob.File), ct);
            var verified = bytes.LongLength == blob.Length && string.Equals(CrmHistorySourceFiles.Hash(bytes), blob.Sha256, StringComparison.OrdinalIgnoreCase);
            proofs.Add(blob.SourceFileId, new CrmHistoryBlobProof(verified, bytes.LongLength));
        }
        return proofs;
    }
}
