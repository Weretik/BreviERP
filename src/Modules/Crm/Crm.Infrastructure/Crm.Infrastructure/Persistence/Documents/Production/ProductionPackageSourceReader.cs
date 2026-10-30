using System.Text.Json;
using Ardalis.Result;
using Crm.Application.CustomSewing.Production.Persistence;
using Crm.Application.CustomSewing.Production.Projection;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Domain.Quotes.Versions;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Documents.Production;

internal sealed class ProductionPackageSourceReader(CrmDbContext db)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    internal async Task<Result<ProductionExportSource>> ReadAsync(Guid quotePublicId, string number,
        QuoteVersion version, IReadOnlyList<Guid> selection, CancellationToken ct)
    {
        var selected = selection.ToHashSet();
        var variants = version.Items.Where(item => item.Included).SelectMany(item => item.Variants)
            .Where(variant => variant.Visible && selected.Contains(variant.PublicId)).ToArray();
        if (variants.Length != selected.Count || variants.Any(x => x.Kind != "customSewing" || x.CustomVersionId is null))
            return Invalid("production_variant_not_selected_or_custom");
        var ids = variants.Select(x => x.CustomVersionId!.Value).Distinct().ToArray();
        var versions = await db.CustomVersions.AsNoTracking().Include(x => x.Draft)
            .Where(x => ids.Contains(x.Id)).ToArrayAsync(ct);
        if (versions.Length != ids.Length) return Invalid("production_custom_version_missing");
        var attachments = await db.Set<CustomVersionAttachment>().AsNoTracking().Include(x => x.File)
            .Where(x => ids.Contains(x.VersionId)).ToArrayAsync(ct);
        var sources = versions.Select(saved => new ProductionCustomVersionSource(saved.Id,
            new QuoteCustomVersionSnapshot(saved.Draft.PublicId, saved.PublicId, saved.Number,
                saved.Configuration, saved.Basis), attachments.Where(x => x.VersionId == saved.Id)
                    .Select(x => new ProductionPinnedFile(x.File.PublicId, x.ContentType, x.Length, x.Sha256, x.Production))
                    .ToArray())).ToArray();
        var byId = sources.ToDictionary(x => x.Id);
        foreach (var variant in variants)
        {
            QuotePricePreview? preview;
            try { preview = JsonSerializer.Deserialize<QuotePricePreview>(variant.PriceBasisJson, Json); }
            catch (JsonException) { return Invalid("production_quote_basis_invalid"); }
            var saved = byId[variant.CustomVersionId!.Value].Snapshot;
            if (preview?.Intent?.Kind != "customSewing" || preview.Intent.CustomVersionPublicId != saved.VersionPublicId ||
                preview.CustomVersion is null || JsonSerializer.Serialize(preview.CustomVersion, Json) != JsonSerializer.Serialize(saved, Json))
                return Invalid("production_quote_custom_snapshot_mismatch");
        }
        return Result.Success(new ProductionExportSource(quotePublicId, number, version, sources));
    }

    private static Result<ProductionExportSource> Invalid(string code) =>
        Result<ProductionExportSource>.Invalid(new ValidationError("productionPackage", code));
}
