using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Composition;
using Catalog.Domain.Pricing.Sewing;
using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.CustomSewing;
using Crm.Domain.Quotes;
using Mediator;
using Reference.Application.Features.Fabric.GetList;
using Reference.Application.Features.Fabric.GetList.DTOs;
using Reference.Application.Features.GarmentAccessory.GetList;
using Reference.Application.Features.GarmentAccessory.GetList.DTOs;
using Reference.Application.Features.GarmentPartOperation.GetList;
using Reference.Application.Features.GarmentPartOperation.GetList.DTOs;

namespace Host.Api.Crm;

/// <summary>
/// Prices a «Виріб з нуля» through the Catalog sewing formula (one calculation per variant: its fabrics plus the shared lines) and names its lines from
/// the Reference lists. Catalog blockers become short reasons the manager can act on.
/// </summary>
internal sealed class QuoteSewingCalculator(ISender sender, TimeProvider clock) : IQuoteSewingCalculator
{
    public async Task<QuoteSewingCalculation> CalculateAsync(
        QuoteSewingComposition composition, CancellationToken cancellationToken)
    {
        var fabrics = (await sender.Send(new GetAllFabricQuery(), cancellationToken)).Value?.ToDictionary(x => x.Id)
            ?? new Dictionary<int, FabricRowDTO>();
        var accessories = (await sender.Send(new GetGarmentAccessoriesQuery(), cancellationToken)).Value?
            .ToDictionary(x => x.Id) ?? new Dictionary<int, GarmentAccessoryRowDTO>();
        var operations = (await sender.Send(new GetGarmentPartOperationsQuery(), cancellationToken)).Value?
            .ToDictionary(x => x.Id) ?? new Dictionary<int, GarmentPartOperationRowDTO>();

        var lines = composition.Lines.Select(line => Info(line, fabrics, accessories, operations)).ToArray();
        var materialLines = composition.Lines.Where(x => x.Kind != QuoteItemLineKind.Operation).ToArray();
        var operationLines = composition.Lines.Where(x => x.Kind == QuoteItemLineKind.Operation).ToArray();
        var materials = materialLines.Select(Material).ToArray();
        var steps = operationLines.Select(x => new SewingCompositionOperation(x.ReferenceId, x.Quantity, x.ManualMinutes))
            .ToArray();
        var at = clock.GetUtcNow();

        var prices = new List<QuoteSewingVariantPrice>(composition.Variants.Count);
        foreach (var variant in composition.Variants)
        {
            var infos = variant.Fabrics.Select(fabric =>
            {
                fabrics.TryGetValue(fabric.FabricId, out var row);
                return new QuoteSewingFabricInfo(fabric.FabricId, fabric.Meters, row?.Name, QuoteCatalogReader.Details(row),
                    row?.Price);
            }).ToArray();
            var query = new CalculateSewingCompositionQuery(
                [.. variant.Fabrics.Select(x =>
                    new SewingCompositionMaterial(SewingCompositionMaterialKind.Fabric, x.FabricId, x.Meters)), .. materials],
                steps, at);
            var result = await sender.Send(query, cancellationToken);
            prices.Add(result.IsSuccess
                ? Price(infos, result.Value.Calculation, materialLines, operationLines, fabrics, accessories, operations)
                : new QuoteSewingVariantPrice(infos, null, null, null, null,
                    [string.Join("; ", result.ValidationErrors.Select(x => x.ErrorMessage)
                        .DefaultIfEmpty("Склад не пройшов перевірку."))]));
            if (result.IsSuccess) lines = WithServicePrices(lines, result.Value.References);
        }

        return new QuoteSewingCalculation(lines, prices);
    }

    private static SewingCompositionMaterial Material(QuoteSewingLine line) => line.Kind switch
    {
        QuoteItemLineKind.Service =>
            new SewingCompositionMaterial(SewingCompositionMaterialKind.OtherCost, 0, line.Quantity, line.CostCode),
        QuoteItemLineKind.Fabric =>
            new SewingCompositionMaterial(SewingCompositionMaterialKind.Fabric, line.ReferenceId, line.Quantity),
        _ => new SewingCompositionMaterial(SewingCompositionMaterialKind.Accessory, line.ReferenceId, line.Quantity)
    };

    /// <summary>A service has no Reference list row: its price and activity come from the calculation's references.</summary>
    private static QuoteSewingLineInfo[] WithServicePrices(QuoteSewingLineInfo[] lines,
        IReadOnlyList<PricingReferenceValue> references) =>
        lines.Select(line =>
        {
            if (line.Kind != QuoteItemLineKind.Service) return line;
            var value = references.FirstOrDefault(x => x.ReferenceKey == $"OtherCost:{line.CostCode}");
            return value is null ? line : line with { UnitValue = value.Value, IsActive = value.IsActive };
        }).ToArray();

    private static QuoteSewingLineInfo Info(QuoteSewingLine line, IReadOnlyDictionary<int, FabricRowDTO> fabrics,
        IReadOnlyDictionary<int, GarmentAccessoryRowDTO> accessories,
        IReadOnlyDictionary<int, GarmentPartOperationRowDTO> operations)
    {
        if (line.Kind == QuoteItemLineKind.Service)
            return new QuoteSewingLineInfo(line.Kind, 0, line.CostCode, "послуга", null, true, line.CostCode);
        if (line.Kind == QuoteItemLineKind.Fabric)
            return fabrics.TryGetValue(line.ReferenceId, out var extra)
                ? new QuoteSewingLineInfo(line.Kind, line.ReferenceId, extra.Name, "м", extra.Price, extra.IsActive)
                : new QuoteSewingLineInfo(line.Kind, line.ReferenceId, null, null, null, false);
        if (line.Kind == QuoteItemLineKind.Accessory)
            return accessories.TryGetValue(line.ReferenceId, out var accessory)
                ? new QuoteSewingLineInfo(line.Kind, line.ReferenceId, accessory.Name, accessory.Unit, accessory.Price,
                    accessory.IsActive)
                : new QuoteSewingLineInfo(line.Kind, line.ReferenceId, null, null, null, false);
        return operations.TryGetValue(line.ReferenceId, out var operation)
            ? new QuoteSewingLineInfo(line.Kind, line.ReferenceId, OperationName(operation), "хв",
                operation.MinutesPerProduct ? null : operation.Min, operation.IsActive)
            : new QuoteSewingLineInfo(line.Kind, line.ReferenceId, null, null, null, false);
    }

    internal static string OperationName(GarmentPartOperationRowDTO operation) =>
        string.IsNullOrWhiteSpace(operation.Variant) ? operation.Name : $"{operation.Name} ({operation.Variant.Trim()})";

    private static QuoteSewingVariantPrice Price(IReadOnlyList<QuoteSewingFabricInfo> variantFabrics,
        SewingArticleCalculation calculation, IReadOnlyList<QuoteSewingLine> materialLines,
        IReadOnlyList<QuoteSewingLine> operationLines, IReadOnlyDictionary<int, FabricRowDTO> fabrics,
        IReadOnlyDictionary<int, GarmentAccessoryRowDTO> accessories,
        IReadOnlyDictionary<int, GarmentPartOperationRowDTO> operations)
    {
        var breakdown = calculation.Breakdown;
        var names = new LineNames(variantFabrics, materialLines, operationLines, fabrics, accessories, operations);
        var problems = calculation.Blockers.Select(x => Problem(x, names)).Distinct().ToArray();
        var tiers = calculation.IsCalculated
            ? new QuoteTierPrices(calculation.Prices!.Retail, calculation.Prices.Wholesale, calculation.Prices.LargeWholesale)
            : null;
        return new QuoteSewingVariantPrice(variantFabrics, breakdown?.TotalMinutes,
            breakdown is null ? null : decimal.Round(breakdown.TotalMinutes * breakdown.MinuteCost, 2),
            breakdown is null ? null : decimal.Round(breakdown.FullCost, 2), tiers, problems);
    }

    /// <summary>
    /// Line keys are `Material:1…n` for the variant's fabrics, `Material:n+1…` for the shared materials in order and `Operation:1…` for
    /// the operations in order; a reference version problem names `Fabric:{id}`, `Accessory:{id}`, `OtherCost:{code}`
    /// or `Operation:{id}`.
    /// </summary>
    private static string Problem(SewingPricingBlocker blocker, LineNames names)
    {
        var line = names.Of(blocker.InputKey, blocker.Code == "AmbiguousReferenceVersion");
        return blocker.Code switch
        {
            "MissingSettings" or "AmbiguousSettings" or "UnsupportedFormula" or "InvalidSchedule" or "InvalidRate"
                or "InvalidMultiplier" =>
                "немає чинних параметрів ціноутворення швейки",
            "MissingFabric" => "оберіть тканину",
            "MissingOperations" => "додайте хоча б одну операцію",
            "MissingMaterialCost" or "UnitMismatch" => $"немає ціни: {line}",
            "UnapprovedReference" => $"вимкнено в довіднику: {line}",
            "MissingOperationTime" => $"вкажіть хвилини операції: {line}",
            "AmbiguousReferenceVersion" => $"кілька чинних цін: {line}",
            _ => line is null ? "не вдалося порахувати ціну" : $"не вдалося порахувати: {line}"
        };
    }

    private sealed record LineNames(
        IReadOnlyList<QuoteSewingFabricInfo> VariantFabrics,
        IReadOnlyList<QuoteSewingLine> MaterialLines,
        IReadOnlyList<QuoteSewingLine> OperationLines,
        IReadOnlyDictionary<int, FabricRowDTO> Fabrics,
        IReadOnlyDictionary<int, GarmentAccessoryRowDTO> Accessories,
        IReadOnlyDictionary<int, GarmentPartOperationRowDTO> Operations)
    {
        public string? Of(string? key, bool referenceKey)
        {
            if (key is null) return null;
            var separator = key.IndexOf(':');
            if (separator < 0) return key;
            var kind = key[..separator];
            var value = key[(separator + 1)..];
            if (kind is "OtherCost") return value;
            if (kind is "Fabric")
                return int.TryParse(value, out var fabricId) && Fabrics.TryGetValue(fabricId, out var f) ? f.Name
                    : VariantFabrics.FirstOrDefault()?.Name ?? "тканина";
            if (kind is "Accessory")
                return int.TryParse(value, out var accessoryId) && Accessories.TryGetValue(accessoryId, out var a)
                    ? a.Name : "фурнітура";
            if (kind is "Operation" && int.TryParse(value, out var operationIdOrLine))
            {
                if (!referenceKey && operationIdOrLine >= 1 && operationIdOrLine <= OperationLines.Count &&
                    Operations.TryGetValue(OperationLines[operationIdOrLine - 1].ReferenceId, out var byLine))
                    return OperationName(byLine);
                return Operations.TryGetValue(operationIdOrLine, out var byId) ? OperationName(byId) : "операція";
            }

            if (kind is "Material" && int.TryParse(value, out var index))
            {
                if (index >= 1 && index <= VariantFabrics.Count) return VariantFabrics[index - 1].Name ?? "тканина";
                var position = index - 1 - VariantFabrics.Count;
                if (position < 0 || position >= MaterialLines.Count) return "матеріал";
                var line = MaterialLines[position];
                return line.Kind switch
                {
                    QuoteItemLineKind.Service => line.CostCode,
                    QuoteItemLineKind.Fabric => Fabrics.TryGetValue(line.ReferenceId, out var extra) ? extra.Name : "тканина",
                    _ => Accessories.TryGetValue(line.ReferenceId, out var accessory) ? accessory.Name : "фурнітура"
                };
            }

            return key;
        }
    }
}
