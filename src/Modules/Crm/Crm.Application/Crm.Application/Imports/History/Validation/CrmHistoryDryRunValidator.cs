using Crm.Application.Imports.History.Contracts;
using FluentValidation;
namespace Crm.Application.Imports.History.Validation;

public sealed class CrmHistoryDryRunValidator : AbstractValidator<CrmHistoryDryRunRequest>
{
    public CrmHistoryDryRunValidator()
    {
        RuleFor(x => x).Must(ValidShape).WithMessage("history_dry_run_invalid_shape");
    }

    private static bool ValidShape(CrmHistoryDryRunRequest? request)
    {
        if (request is null || request.Snapshot is null || request.Options is null) return false;
        var source = request.Snapshot;
        var options = request.Options;
        if (!CrmHistoryInputRules.Identifier(source.DatasetId, 80) || !CrmHistoryInputRules.Hash(source.DatabaseSha256) ||
            !CrmHistoryInputRules.Hash(source.ManifestSha256) || !CrmHistoryInputRules.OptionalHash(source.ControlSha256) ||
            !CrmHistoryInputRules.OptionalHash(source.OptionsSha256) || source.Tables is null || source.Rows is null ||
            !ValidIssues(source.Issues) || source.Tables.Count > 12 || source.Rows.Count > CrmHistoryInputRules.MaximumRows ||
            options.OwnerMappings is null || options.ExactIdentityMappings is null || options.PreviousStaging is null ||
            options.OwnerMappings.Count > CrmHistoryInputRules.MaximumRows || options.ExactIdentityMappings.Count > CrmHistoryInputRules.MaximumRows ||
            options.PreviousStaging.Count > CrmHistoryInputRules.MaximumLedgerEntries ||
            !ValidReference(options.ScopeApprovalReference) || !ValidReference(options.RetentionPolicyReference)) return false;
        if (options.PreviousStaging.Count != 0 && options.StagingDatasetId != source.DatasetId) return false;
        var tableNames = new HashSet<string>(StringComparer.Ordinal);
        long actualRows = 0;
        foreach (var table in source.Tables)
        {
            if (table is null || !CrmHistoryInputRules.Entities.Contains(table.Entity) || !tableNames.Add(table.Entity) ||
                table.ExpectedRows < 0 || table.ActualRows < 0 || table.ActualRows > CrmHistoryInputRules.MaximumRows || !ValidIssues(table.Issues)) return false;
            actualRows += table.ActualRows;
        }
        if (actualRows > CrmHistoryInputRules.MaximumRows) return false;
        var ownerKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in options.OwnerMappings)
            if (!CrmHistoryInputRules.Hash(mapping.Key) || mapping.Value <= 0 || !ownerKeys.Add(mapping.Key)) return false;
        var identityKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in options.ExactIdentityMappings)
            if (!CrmHistoryInputRules.Hash(mapping.Key) || !Guid.TryParseExact(mapping.Value, "D", out var id) || id == Guid.Empty || !identityKeys.Add(mapping.Key)) return false;
        var ledger = new Dictionary<(string, string), string>();
        foreach (var entry in options.PreviousStaging)
        {
            if (entry is null || !CrmHistoryInputRules.Entities.Contains(entry.Entity) || !CrmHistoryInputRules.Identifier(entry.SourceId) || !CrmHistoryInputRules.Hash(entry.PayloadSha256)) return false;
            var key = (entry.Entity, entry.SourceId);
            if (ledger.TryGetValue(key, out var previous) && !previous.Equals(entry.PayloadSha256, StringComparison.OrdinalIgnoreCase)) return false;
            ledger[key] = entry.PayloadSha256;
        }
        var ledgerKeys = ledger.Keys.ToHashSet();
        long referenceCount = 0;
        foreach (var row in source.Rows)
        {
            if (row is null || !CrmHistoryInputRules.Entities.Contains(row.Entity) || !CrmHistoryInputRules.BoundedText(row.SourceId, 512) ||
                !CrmHistoryInputRules.Hash(row.PayloadSha256) || row.Roles is null || row.Roles.Count > 16 ||
                row.Roles.Any(x => !CrmHistoryInputRules.BoundedText(x, 64)) || !ValidHashList(row.OwnerKeys) ||
                !ValidHashList(row.ExternalIdentityKeys) || row.References is null || row.References.Count > CrmHistoryLimits.MaximumRowReferences ||
                row.Kind is not null && !CrmHistoryInputRules.BoundedText(row.Kind, 128) || !ValidIssues(row.Issues)) return false;
            foreach (var reference in row.References)
                if (reference is null || !CrmHistoryInputRules.Entities.Contains(reference.Entity) || !CrmHistoryInputRules.BoundedText(reference.SourceId, 512)) return false;
            referenceCount += row.References.Count;
            if (referenceCount > CrmHistoryInputRules.MaximumReferences) return false;
            if (CrmHistoryInputRules.Identifier(row.SourceId)) ledgerKeys.Add((row.Entity, row.SourceId));
        }
        return ledgerKeys.Count <= CrmHistoryInputRules.MaximumLedgerEntries;
    }

    private static bool ValidReference(string? value) => value is null || CrmHistoryInputRules.BoundedText(value, 256);
    private static bool ValidIssues(IReadOnlyList<string>? issues) => issues is not null && issues.Count <= 32 &&
        issues.All(x => CrmHistoryInputRules.BoundedText(x, 256));
    private static bool ValidHashList(IReadOnlyList<string>? hashes) => hashes is not null && hashes.Count <= 64 && hashes.All(CrmHistoryInputRules.Hash);
}
