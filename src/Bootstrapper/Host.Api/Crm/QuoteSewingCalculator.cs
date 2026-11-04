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
/// Prices a «Виріб з нуля» through the Catalog sewing formula (one calculation per fabric) and names its lines from
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

        var lines = composition.Lines.Select(line => Info(line, accessories, operations)).ToArray();
        var accessoryLines = composition.Lines.Where(x => x.Kind == QuoteItemLineKind.Accessory).ToArray();
        var operationLines = composition.Lines.Where(x => x.Kind == QuoteItemLineKind.Operation).ToArray();
        var materials = accessoryLines.Select(x =>
            new SewingCompositionMaterial(SewingCompositionMaterialKind.Accessory, x.ReferenceId, x.Quantity)).ToArray();
        var steps = operationLines.Select(x => new SewingCompositionOperation(x.ReferenceId, x.Quantity, x.ManualMinutes))
            .ToArray();
        var at = clock.GetUtcNow();

        var prices = new List<QuoteSewingFabricPrice>(composition.Fabrics.Count);
        foreach (var fabric in composition.Fabrics)
        {
            fabrics.TryGetValue(fabric.FabricId, out var row);
            var query = new CalculateSewingCompositionQuery(
                [new SewingCompositionMaterial(SewingCompositionMaterialKind.Fabric, fabric.FabricId, fabric.Meters), .. materials],
                steps, at);
            var result = await sender.Send(query, cancellationToken);
            prices.Add(result.IsSuccess
                ? Price(fabric, row, result.Value.Calculation, accessoryLines, operationLines, accessories, operations)
                : new QuoteSewingFabricPrice(fabric.FabricId, fabric.Meters, row?.Name, QuoteCatalogReader.Details(row),
                    row?.Price, null, null, null, null, [string.Join("; ", result.ValidationErrors.Select(x => x.ErrorMessage)
                        .DefaultIfEmpty("Склад не пройшов перевірку."))]));
        }

        return new QuoteSewingCalculation(lines, prices);
    }

    private static QuoteSewingLineInfo Info(QuoteSewingLine line,
        IReadOnlyDictionary<int, GarmentAccessoryRowDTO> accessories,
        IReadOnlyDictionary<int, GarmentPartOperationRowDTO> operations)
    {
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

    private static string OperationName(GarmentPartOperationRowDTO operation) =>
        string.IsNullOrWhiteSpace(operation.Variant) ? operation.Name : $"{operation.Name} ({operation.Variant.Trim()})";

    private static QuoteSewingFabricPrice Price(QuoteSewingFabric fabric, FabricRowDTO? row,
        SewingArticleCalculation calculation, IReadOnlyList<QuoteSewingLine> accessoryLines,
        IReadOnlyList<QuoteSewingLine> operationLines, IReadOnlyDictionary<int, GarmentAccessoryRowDTO> accessories,
        IReadOnlyDictionary<int, GarmentPartOperationRowDTO> operations)
    {
        var breakdown = calculation.Breakdown;
        var problems = calculation.Blockers
            .Select(x => Problem(x, row, accessoryLines, operationLines, accessories, operations)).Distinct().ToArray();
        var tiers = calculation.IsCalculated
            ? new QuoteTierPrices(calculation.Prices!.Retail, calculation.Prices.Wholesale, calculation.Prices.LargeWholesale)
            : null;
        return new QuoteSewingFabricPrice(fabric.FabricId, fabric.Meters, row?.Name, QuoteCatalogReader.Details(row),
            row?.Price, breakdown?.TotalMinutes,
            breakdown is null ? null : decimal.Round(breakdown.TotalMinutes * breakdown.MinuteCost, 2),
            breakdown is null ? null : decimal.Round(breakdown.FullCost, 2), tiers, problems);
    }

    /// <summary>
    /// Line keys are `Material:1` for the fabric, `Material:2…` for the accessories in order and `Operation:1…` for
    /// the operations in order; a reference version problem names `Fabric:{id}`, `Accessory:{id}` or `Operation:{id}`.
    /// </summary>
    private static string Problem(SewingPricingBlocker blocker, FabricRowDTO? fabric,
        IReadOnlyList<QuoteSewingLine> accessoryLines, IReadOnlyList<QuoteSewingLine> operationLines,
        IReadOnlyDictionary<int, GarmentAccessoryRowDTO> accessories,
        IReadOnlyDictionary<int, GarmentPartOperationRowDTO> operations)
    {
        var line = LineName(blocker.InputKey, blocker.Code == "AmbiguousReferenceVersion", fabric, accessoryLines,
            operationLines, accessories, operations);
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

    private static string? LineName(string? key, bool referenceKey, FabricRowDTO? fabric, IReadOnlyList<QuoteSewingLine> accessoryLines,
        IReadOnlyList<QuoteSewingLine> operationLines, IReadOnlyDictionary<int, GarmentAccessoryRowDTO> accessories,
        IReadOnlyDictionary<int, GarmentPartOperationRowDTO> operations)
    {
        if (key is null) return null;
        var parts = key.Split(':');
        if (parts.Length != 2) return key;
        if (parts[0] is "Fabric") return fabric?.Name ?? "тканина";
        if (parts[0] is "Accessory")
            return int.TryParse(parts[1], out var accessoryId) && accessories.TryGetValue(accessoryId, out var a)
                ? a.Name : "фурнітура";
        if (parts[0] is "Operation" && int.TryParse(parts[1], out var operationIdOrLine))
        {
            if (!referenceKey && operationIdOrLine >= 1 && operationIdOrLine <= operationLines.Count &&
                operations.TryGetValue(operationLines[operationIdOrLine - 1].ReferenceId, out var byLine))
                return OperationName(byLine);
            return operations.TryGetValue(operationIdOrLine, out var byId) ? OperationName(byId) : "операція";
        }

        if (parts[0] is "Material" && int.TryParse(parts[1], out var index))
        {
            if (index == 1) return fabric?.Name ?? "тканина";
            var position = index - 2;
            return position >= 0 && position < accessoryLines.Count &&
                   accessories.TryGetValue(accessoryLines[position].ReferenceId, out var accessory)
                ? accessory.Name
                : "фурнітура";
        }

        return key;
    }
}
