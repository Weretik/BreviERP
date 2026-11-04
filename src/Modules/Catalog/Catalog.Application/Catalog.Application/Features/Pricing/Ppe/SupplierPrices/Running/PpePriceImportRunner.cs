using System.Globalization;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Shared;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;
using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Running;

/// <summary>Who started an update and how; <see cref="Trigger"/> is shown in «Останнє оновлення».</summary>
public sealed record PpePriceImportRequest(bool DryRun, string Trigger, string Actor, string ActorName);

/// <summary>Reads the supplier's prices for the products that can change (active, without a manual price).</summary>
public delegate Task<Result<IReadOnlyList<SupplierFilePrice>>> PpeSupplierPriceRead(
    IReadOnlyList<PpePriceImportProduct> products, CancellationToken cancellationToken);

/// <summary>
/// One update of a source, whatever brings the prices (files, websites, Google CSV): finds the supplier and its
/// products, plans the new prices, and unless it is a dry run saves each changed price on the card and as a new
/// approved offer version, then records the run (CAT-017 R-07).
/// </summary>
public sealed class PpePriceImportRunner(
    IPpePriceSourceStore store,
    IProductReferenceReader references,
    ICatalogRepository<ProductEntity> products,
    IPpePricingInputReader inputs,
    IPpeOfferVersionWriter offers,
    TimeProvider timeProvider)
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public async Task<Result<PpePriceImportResult>> RunAsync(
        PpePriceSource source, PpeSupplierPriceRead read, PpePriceImportRequest request, CancellationToken cancellationToken)
    {
        var settings = (await store.ReadSettingsAsync(cancellationToken)).FirstOrDefault(x => x.Key == source.Key);
        var supplier = PpePriceSourceSupplier.Resolve(source, settings,
            (await references.GetSnapshotAsync(cancellationToken)).Suppliers);
        if (supplier is null)
            return Result.Invalid(new ValidationError("supplierId",
                $"Постачальника «{source.Title}» не знайдено в довіднику «Постачальники». Оберіть його в «Імпорт цін»."));

        var entities = await products.ListAsync(new PpeSupplierProductsSpec(supplier.Id), cancellationToken);
        var current = (await inputs.ReadAsync(entities.Select(x => x.Id.Value).ToList(), cancellationToken))
            .ToDictionary(x => x.ProductId);
        var planned = entities.Select(entity =>
        {
            var offer = current.GetValueOrDefault(entity.Id.Value)?.Offer;
            var profile = entity.CatalogProfile!;
            return new PpePriceImportProduct(entity.Id.Value, profile.SaleSku, entity.Name, profile.IsActive,
                profile.AllowManualPrice, profile.SupplierSku, profile.SupplierProductUrl,
                offer?.Values.SupplierCost ?? profile.SupplierPrice, offer?.Values.ExternalWholesale,
                offer?.Values.ExternalRetail);
        }).ToList();

        var prices = await read(planned.Where(x => x.IsActive && !x.AllowManualPrice).ToList(), cancellationToken);
        if (!prices.IsSuccess) return Result.Invalid(prices.ValidationErrors.ToList());

        var discount = settings?.DiscountPercent ?? source.DefaultDiscountPercent;
        var rows = PpePriceImportPlanner.Plan(source, discount, planned, prices.Value).ToList();
        if (!request.DryRun)
            await ApplyAsync(source, entities, current, rows, request.Actor, cancellationToken);

        var result = Summarize(source.Key, !request.DryRun, discount, rows);
        if (!request.DryRun)
            await store.RecordRunAsync(new PpePriceImportRun(source.Key, request.Trigger, request.ActorName,
                timeProvider.GetUtcNow(), result.Checked, result.Changed, result.Unchanged, result.Skipped,
                rows.Where(IsProblem)
                    .Select(row => new PpePriceImportProblem(row.Code, row.Name, row.Message ?? string.Empty))
                    .ToList()), cancellationToken);
        return Result.Success(result);
    }

    private async Task ApplyAsync(PpePriceSource source, IReadOnlyList<ProductEntity> entities,
        IReadOnlyDictionary<int, PpePricingProductInput> current, List<PpePriceImportRow> rows, string actor,
        CancellationToken cancellationToken)
    {
        var sourceLabel = $"Імпорт цін: {source.Title}";
        var today = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), Kyiv)
            .ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        for (var index = 0; index < rows.Count; index++)
        {
            if (rows[index].Status != PpePriceImportStatus.Changed) continue;
            var saved = await SaveAsync(entities.Single(x => x.Id.Value == rows[index].ProductId),
                current.GetValueOrDefault(rows[index].ProductId)?.Offer, rows[index], sourceLabel, today, actor,
                cancellationToken);
            if (!saved)
                rows[index] = rows[index] with
                {
                    Status = PpePriceImportStatus.Invalid,
                    Message = "Ціну товару щойно змінили в картці. Оновіть ще раз."
                };
        }
    }

    private async Task<bool> SaveAsync(ProductEntity product, PpeOfferVersion? offer, PpePriceImportRow row,
        string sourceLabel, string today, string actor, CancellationToken cancellationToken)
    {
        var profile = product.CatalogProfile!;
        var ruleCode = offer?.Values.RuleCode ?? profile.PricingRuleCode;
        var previousPrice = profile.SupplierPrice;
        SetPrice(product, row.NewPrice);

        // The writer saves the tracked card change in the same transaction as the new offer version.
        var appended = await offers.AppendAsync(product.Id.Value, offer?.Id, Guid.NewGuid(),
            new PpeOfferValues(ruleCode, row.NewPrice, row.NewRetail, row.NewWholesale, sourceLabel, today),
            actor, cancellationToken);
        if (appended.IsSuccess) return true;

        // Put the card price back so a later save of this context does not write it without its offer version.
        SetPrice(product, previousPrice);
        return false;
    }

    private void SetPrice(ProductEntity product, decimal? price)
    {
        var profile = product.CatalogProfile!;
        product.SetPpeSupplier(new PpeSupplierDetails(profile.SupplierId!.Value, profile.SupplierSku,
            profile.SupplierProductUrl, price, profile.AllowManualPrice, profile.PricingRuleCode, profile.Unit),
            timeProvider.GetUtcNow());
    }

    private static bool IsProblem(PpePriceImportRow row)
        => row.Status is PpePriceImportStatus.NotInFile or PpePriceImportStatus.Invalid;

    private static PpePriceImportResult Summarize(string key, bool applied, decimal discount, IReadOnlyList<PpePriceImportRow> rows)
        => new(key, applied, discount, rows.Count,
            rows.Count(x => x.Status == PpePriceImportStatus.Changed),
            rows.Count(x => x.Status == PpePriceImportStatus.Unchanged),
            rows.Count(x => x.Status is PpePriceImportStatus.ManualPrice or PpePriceImportStatus.Inactive),
            rows);
}
