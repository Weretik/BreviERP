using Ardalis.Result;
using Crm.Application.CustomSewing.Production.Contracts;
using Crm.Domain.CustomSewing;
using Crm.Domain.Quotes.Versions;

namespace Crm.Application.CustomSewing.Production.Projection;

/// <summary>Pure allowlist projection. Access and ready/issue checks belong to the source reader/use case.</summary>
public static class ProductionPackageProjectionBuilder
{
    public static Result<ProductionPackageManifest> Build(Guid quotePublicId, string quoteNumber,
        QuoteVersion quoteVersion, IReadOnlyList<Guid> selectedVariantPublicIds,
        IReadOnlyList<ProductionCustomVersionSource> customVersions)
    {
        if (quotePublicId == Guid.Empty || quoteVersion is null ||
            string.IsNullOrWhiteSpace(quoteNumber) || quoteNumber.Length > 80 ||
            quoteVersion.Currency != "UAH" || selectedVariantPublicIds is null ||
            selectedVariantPublicIds.Count is < 1 or > 3000 ||
            selectedVariantPublicIds.Any(x => x == Guid.Empty) ||
            selectedVariantPublicIds.Distinct().Count() != selectedVariantPublicIds.Count ||
            customVersions is null || customVersions.Count > 3000 ||
            customVersions.Any(x => x is null || x.Id.Value <= 0 || x.Snapshot is null || x.Files is null) ||
            customVersions.Select(x => x.Id).Distinct().Count() != customVersions.Count)
            return Invalid("production_selection_invalid");

        var selected = selectedVariantPublicIds.ToHashSet();
        var variants = quoteVersion.Items.Where(item => item.Included).OrderBy(item => item.SortOrder)
            .SelectMany(item => item.Variants.Where(variant => variant.Visible).OrderBy(variant => variant.SortOrder))
            .Where(variant => selected.Contains(variant.PublicId)).ToArray();
        if (variants.Length != selected.Count || variants.Any(x => x.Kind != "customSewing" || x.CustomVersionId is null))
            return Invalid("production_variant_not_selected_or_custom");

        var sources = customVersions.ToDictionary(x => x.Id);
        var options = new List<ProductionOption>(variants.Length);
        var files = new Dictionary<Guid, ProductionPackageFile>();
        foreach (var variant in variants)
        {
            if (!sources.TryGetValue(variant.CustomVersionId!.Value, out var source))
                return Invalid("production_custom_version_missing");
            var snapshot = source.Snapshot;
            if (snapshot.DraftPublicId == Guid.Empty || snapshot.VersionPublicId == Guid.Empty || snapshot.Number <= 0 ||
                snapshot.Configuration is null || snapshot.Basis is null || snapshot.Basis.Blockers is null || snapshot.Basis.References is null ||
                !snapshot.Basis.IsCalculated || string.IsNullOrWhiteSpace(snapshot.Basis.FormulaVersion))
                return Invalid("production_snapshot_invalid");

            Crm.Domain.CustomSewing.Composition.CustomConfiguration configuration;
            try { configuration = snapshot.Configuration.Freeze(); }
            catch (CustomSewingValidationException) { return Invalid("production_snapshot_invalid"); }
            if (configuration.Fabrics.Count == 0 || configuration.Operations.Count == 0)
                return Invalid("production_composition_incomplete");
            var breakdown = snapshot.Basis.Breakdown!;
            if (breakdown.TotalMinutes <= 0 || breakdown.MinuteCost <= 0 ||
                new[] { breakdown.Seamstress, breakdown.Cutting, breakdown.Master, breakdown.Bonus }.Any(x => x < 0))
                return Invalid("production_wage_basis_invalid");

            var pinnedFiles = ProductionPackageFileProjection.Build(source.Files);
            if (!pinnedFiles.IsSuccess) return Result<ProductionPackageManifest>.Invalid(pinnedFiles.ValidationErrors);
            foreach (var file in pinnedFiles.Value)
            {
                if (files.TryGetValue(file.FilePublicId, out var previous) && previous != file)
                    return Invalid("production_file_provenance_conflict");
                files[file.FilePublicId] = file;
                if (files.Count > 30) return Invalid("production_files_invalid");
            }
            var allowedIds = pinnedFiles.Value.Select(file => file.FilePublicId).ToHashSet();
            if (configuration.Applications.Any(app => app.ArtworkFileId is { } id && !allowedIds.Contains(id)))
                return Invalid("production_artwork_not_allowed");

            var operations = ProductionOperationProjection.Build(configuration.Operations, snapshot.Basis.References);
            if (!operations.IsSuccess) return Result<ProductionPackageManifest>.Invalid(operations.ValidationErrors);
            if (operations.Value.Sum(x => x.Quantity * x.EffectiveMinutes) != breakdown.TotalMinutes)
                return Invalid("production_operation_total_mismatch");
            options.Add(new ProductionOption(variant.PublicId, snapshot.DraftPublicId, snapshot.VersionPublicId,
                snapshot.Number, configuration.Name, configuration.ProductType, configuration.Season,
                Array.AsReadOnly(configuration.Fabrics.Select(x => new ProductionMaterial(x.ReferenceId, x.Quantity, x.Unit)).ToArray()),
                Array.AsReadOnly(configuration.Accessories.Select(x => new ProductionMaterial(x.ReferenceId, x.Quantity, x.Unit)).ToArray()),
                operations.Value,
                Array.AsReadOnly(configuration.Applications.Select(x => new ProductionApplication(x.Kind, x.Location, x.ArtworkFileId)).ToArray()),
                snapshot.Basis.FormulaVersion, snapshot.Basis.SettingsVersionId!.Value,
                new ProductionWageBasis(breakdown.TotalMinutes, breakdown.MinuteCost, breakdown.Seamstress,
                    breakdown.Cutting, breakdown.Master, breakdown.Bonus),
                Array.AsReadOnly(allowedIds.OrderBy(x => x).ToArray())));
        }
        return Result.Success(new ProductionPackageManifest(1, "brevi.production.v1", quotePublicId,
            quoteVersion.PublicId, quoteVersion.VersionNumber, quoteNumber.Trim(), "UAH",
            options.AsReadOnly(), Array.AsReadOnly(files.Values.OrderBy(file => file.FilePublicId).ToArray())));
    }

    private static Result<ProductionPackageManifest> Invalid(string code) =>
        Result<ProductionPackageManifest>.Invalid(new ValidationError("productionPackage", code));
}
