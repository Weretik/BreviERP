using BuildingBlocks.Domain.Errors;
using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Product.Shared.Overview;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Sewing.ValueObjects;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <summary>
/// Compares the sheet with the stored products and decides what to create, update, keep and delete. Pure: it reads
/// only the <see cref="SheetImportContext"/> and changes no stored product. Every problem of the body is reported in
/// one pass.
/// </summary>
public static class SheetImportPlanner
{
    public const string OfferSource = "Імпорт з таблиці";
    private static readonly ProductId ProbeId = ProductId.Create(1_000_000_000);
    private static readonly ProductSlug ProbeSlug = ProductSlug.Create("probe").Value;

    public static SheetImportPlan Plan(ImportSheetProductsCommand command, SheetImportContext context)
    {
        var log = new SheetImportIssueLog();
        var today = SheetDates.Today(context.Now);
        var references = new SheetReferenceResolver(context.References);
        var sewingBuilder = new SheetSewingDraftBuilder(references, log);
        var ppeBuilder = new SheetPpeDraftBuilder(references, context.KnownRuleCodes, log);
        var matcher = new SheetProductMatcher(context.Products);
        var flagged = SheetBodyDuplicates.Find(command, log);

        var rows = new List<(SheetProductDraft Draft, ProductEntity? Existing, SheetPpeRow? Ppe)>();
        var matchedBy = new Dictionary<ProductEntity, int>(ReferenceEqualityComparer.Instance);
        void Add(ProductType type, int index, int rowNumber, string code, string name,
            Func<SheetProductDraft?> build, SheetPpeRow? ppe)
        {
            if (flagged.Contains((type, index))) return;
            var existing = matcher.Match(type, code, name);
            if (existing is not null && !matchedBy.TryAdd(existing, rowNumber))
            {
                log.Problem(SheetImportPlan.Scope(type), rowNumber, code.Trim(),
                    $"Рядок збігається з тим самим товаром «{existing.Name}» (ID {existing.Id.Value}), що й рядок {matchedBy[existing]}.");
                return;
            }
            var draft = build();
            if (draft is not null) rows.Add((draft, existing, ppe));
        }
        for (var i = 0; i < command.Sewing.Count; i++)
        {
            var row = command.Sewing[i];
            Add(ProductType.Sewing, i, row.RowNumber, row.Code, row.Name, () => sewingBuilder.Build(row), null);
        }
        for (var i = 0; i < command.Ppe.Count; i++)
        {
            var row = command.Ppe[i];
            Add(ProductType.Ppe, i, row.RowNumber, row.Code, row.Name, () => ppeBuilder.Build(row), row);
        }

        var unmatched = context.Products.Where(x => !matchedBy.ContainsKey(x)).ToList();
        var deletes = command.DeleteMissing ? unmatched : [];
        foreach (var product in deletes.Where(x => context.CalculatedProductIds.Contains(x.Id.Value)))
            log.Problem(SheetImportPlan.Scope(product.Type), null, product.Code,
                $"Товар «{product.Name}» (ID {product.Id.Value}) немає в таблиці, але він має збережені розрахунки ціни: " +
                "видалити його не можна. Додайте модель у таблицю або спершу приберіть її розрахунки.");
        var survivors = command.DeleteMissing ? [] : unmatched;
        ReportSurvivorClashes(rows.Select(x => x.Draft).ToList(), survivors, matchedBy.Keys, log);
        ReportRemovedCalculatedArticles(rows, context.CalculatedArticleIds, log);

        var nextId = context.Products.Count == 0 ? 1 : context.Products.Max(x => x.Id.Value) + 1;
        var deleted = deletes.ToHashSet(ReferenceEqualityComparer.Instance);
        var slugs = context.Products.Where(x => !deleted.Contains(x)).Select(x => x.Slug.Value).ToHashSet(StringComparer.Ordinal);
        var writes = new List<SheetPlannedProduct>();
        var counts = new Dictionary<ProductType, (int Created, int Updated, int Unchanged)>();
        foreach (var (draft, existing, ppe) in rows.OrderBy(x => x.Draft.Type).ThenBy(x => x.Draft.RowNumber))
        {
            var probe = Probe(draft, existing, today, context.Now, log);
            if (probe is null) continue;
            var offer = ppe is null ? null
                : Offer(draft, ppe, existing is null ? null : context.CurrentOffers.GetValueOrDefault(existing.Id.Value), today);
            var changed = existing is null || offer is not null
                || SheetProductFingerprint.Of(existing) != SheetProductFingerprint.Of(probe);
            var count = counts.GetValueOrDefault(draft.Type);
            counts[draft.Type] = existing is null ? count with { Created = count.Created + 1 }
                : changed ? count with { Updated = count.Updated + 1 } : count with { Unchanged = count.Unchanged + 1 };
            if (!changed) continue;

            var id = existing?.Id ?? ProductId.Create(nextId++);
            var slug = existing?.Slug.Value ?? FreeSlug(draft, slugs, context);
            writes.Add(new SheetPlannedProduct(existing, id, slug, draft, offer));
        }

        return new SheetImportPlan(writes, deletes, Counts(counts, ProductType.Sewing), Counts(counts, ProductType.Ppe),
            log.Problems, log.Warnings, today);
    }

    /// <summary>Applies the draft to a throwaway product; a domain rule failure becomes a problem.</summary>
    private static ProductEntity? Probe(SheetProductDraft draft, ProductEntity? existing, DateOnly today,
        DateTimeOffset now, SheetImportIssueLog log)
    {
        DomainResult checkedDraft;
        ProductEntity? probe = null;
        try
        {
            var created = ProductEntity.Create(existing?.Id ?? ProbeId, draft.Name, draft.Name,
                existing?.Slug ?? ProbeSlug, draft.Type, now);
            checkedDraft = created.IsFailure ? created.Error! : DomainResult.Success();
            if (created.IsSuccess)
            {
                probe = created.Value;
                checkedDraft = SheetProductApplier.ApplyCard(probe, draft, now, index => ArticleId.Create(index + 1));
                if (checkedDraft.IsSuccess && draft.Sewing is { } sewing)
                    checkedDraft = SheetProductApplier.ApplyComposition(probe, sewing, now);
            }
        }
        catch (ArgumentException exception)
        {
            // A sheet value that a guard of a type refuses (a reference id of zero and the like).
            checkedDraft = DomainResult.Failure(new Error("Catalog.SheetImport.Value", exception.Message));
        }

        if (checkedDraft.IsSuccess) return probe;
        log.Problem(SheetImportPlan.Scope(draft.Type), draft.RowNumber, draft.Code,
            $"Дані не пройшли перевірку: {checkedDraft.Error!.Message}");
        return null;
    }

    /// <summary>
    /// A new PPE offer version when the supplier price, the rule or a given Insight price differs from the current
    /// one. No supplier price means no version. Insight prices missing in the sheet keep the current ones.
    /// </summary>
    private static SheetOfferPlan? Offer(SheetProductDraft draft, SheetPpeRow row, PpeOfferVersion? current, DateOnly today)
    {
        if (draft.Ppe?.SupplierPrice is not { } price) return null;
        var rule = draft.Ppe.PricingRuleCode;
        var retail = row.InsightRetail ?? current?.Values.ExternalRetail;
        var wholesale = row.InsightWholesale ?? current?.Values.ExternalWholesale;
        if (current is not null && current.Values.SupplierCost == price && current.Values.RuleCode == rule
            && current.Values.ExternalRetail == retail && current.Values.ExternalWholesale == wholesale)
            return null;
        return new SheetOfferPlan(current?.Id,
            new PpeOfferValues(rule, price, retail, wholesale, OfferSource, SheetDates.Format(today)));
    }

    /// <summary>Names, model codes and sale SKUs that stored products outside the sheet keep (no deleteMissing).</summary>
    private static void ReportSurvivorClashes(IReadOnlyList<SheetProductDraft> drafts, IReadOnlyList<ProductEntity> survivors,
        IEnumerable<ProductEntity> matched, SheetImportIssueLog log)
    {
        var names = new Dictionary<string, ProductEntity>(StringComparer.Ordinal);
        var codes = new Dictionary<string, ProductEntity>(StringComparer.Ordinal);
        var skus = new Dictionary<string, ProductEntity>(StringComparer.Ordinal);
        foreach (var product in survivors)
        {
            names.TryAdd(product.Name, product);
            names.TryAdd(product.RuName, product);
            if (product.ModelCode is { } code) codes.TryAdd(SheetReferenceResolver.NormalizeCode(code), product);
            foreach (var article in product.Articles) skus.TryAdd(article.SaleSku, product);
        }
        // A sale SKU in a profile stays as it is unless the product is PPE in the sheet.
        foreach (var product in survivors.Concat(matched.Where(x => x.Type == ProductType.Sewing)))
            if (product.CatalogProfile?.SaleSku is { } sku) skus.TryAdd(sku, product);

        foreach (var draft in drafts)
        {
            void Clash(ProductEntity other, string what) => log.Problem(SheetImportPlan.Scope(draft.Type), draft.RowNumber,
                draft.Code, $"{what} уже має товар «{other.Name}» (ID {other.Id.Value}), який не оновлюється цим імпортом.");
            if (names.TryGetValue(draft.Name, out var byName)) Clash(byName, $"Назву «{draft.Name}»");
            if (draft.Type == ProductType.Sewing && codes.TryGetValue(draft.Code, out var byCode))
                Clash(byCode, $"Код {draft.Code}");
            var draftSkus = draft.Type == ProductType.Ppe ? [draft.Code] : draft.Sewing!.Articles.Select(x => x.SaleSku);
            foreach (var sku in draftSkus)
                if (skus.TryGetValue(sku, out var bySku)) Clash(bySku, $"Артикул на сайті {sku}");
        }
    }

    /// <summary>An article left out of the sheet is removed; saved calculations restrict that.</summary>
    private static void ReportRemovedCalculatedArticles(
        IEnumerable<(SheetProductDraft Draft, ProductEntity? Existing, SheetPpeRow? Ppe)> rows,
        IReadOnlySet<long> calculatedArticleIds, SheetImportIssueLog log)
    {
        foreach (var (draft, existing, _) in rows)
        {
            if (existing is null || draft.Sewing is null) continue;
            var kept = draft.Sewing.Articles.Select(x => x.SaleSku).ToHashSet(StringComparer.Ordinal);
            foreach (var article in existing.Articles.Where(x => !kept.Contains(x.SaleSku) && calculatedArticleIds.Contains(x.Id.Value)))
                log.Problem("sewing", draft.RowNumber, draft.Code,
                    $"Артикула {article.SaleSku} немає в таблиці, але він має збережені розрахунки ціни: видалити його не можна.");
        }
    }

    private static string FreeSlug(SheetProductDraft draft, HashSet<string> taken, SheetImportContext context)
    {
        var slug = ProductSlugCandidates.For(context.SlugGenerator, draft.Name, draft.Code).FirstOrDefault(x => !taken.Contains(x))
            ?? ProductSlugCandidates.Fallback();
        taken.Add(slug);
        return slug;
    }

    private static SheetImportCounts Counts(Dictionary<ProductType, (int Created, int Updated, int Unchanged)> counts, ProductType type)
    {
        var count = counts.GetValueOrDefault(type);
        return new SheetImportCounts(count.Created, count.Updated, count.Unchanged);
    }
}
