using Crm.Domain.Quotes;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Application.Abstractions.Quotes.Products;

namespace Crm.Application.Features.Quotes.Shared;

/// <summary>
/// The composition the «Виріб з нуля» panel prices before it goes into the КП: each variant is a set of fabrics whose
/// cost is summed with the shared lines (R-26).
/// </summary>
public sealed record QuoteCustomSewingPriceInput(
    IReadOnlyList<QuoteCustomSewingVariantInput> Variants, IReadOnlyList<QuoteItemLineInput> Lines);

public sealed record QuoteCustomSewingVariantInput(IReadOnlyList<QuoteOptionFabricInput> Fabrics);

public sealed record QuoteCustomSewingPriceResponse(
    IReadOnlyList<QuoteCustomSewingLineResponse> Lines, IReadOnlyList<QuoteCustomSewingVariantResponse> Variants);

public sealed record QuoteCustomSewingLineResponse(
    string Kind, int ReferenceId, string? Name, string? Unit, decimal? UnitValue, bool IsActive, string? CostCode);

/// <param name="Title">«Грета + Сітка»; null when a fabric is gone.</param>
public sealed record QuoteCustomSewingVariantResponse(
    string? Title,
    IReadOnlyList<QuoteCustomSewingFabricResponse> Fabrics,
    decimal? Minutes,
    decimal? LaborCost,
    decimal? FullCost,
    QuoteTierPricesResponse? Prices,
    IReadOnlyList<string> Problems);

/// <param name="PricePerMeter">For the manager only.</param>
public sealed record QuoteCustomSewingFabricResponse(
    int FabricId, decimal Meters, string? Name, string? Details, decimal? PricePerMeter);

internal static class QuoteCustomSewingMapper
{
    public static QuoteSewingLine ToLine(this QuoteItemLineInput input) =>
        new(Enum.Parse<QuoteItemLineKind>(input.Kind), input.ReferenceId, input.Quantity, input.ManualMinutes,
            input.CostCode?.Trim());

    public static QuoteSewingVariant ToVariant(this IReadOnlyList<QuoteOptionFabricInput> fabrics) =>
        new(fabrics.Select(x => new QuoteSewingFabric(x.FabricId, x.Meters)).ToArray());

    public static QuoteCustomSewingPriceResponse ToResponse(this QuoteSewingCalculation calculation) => new(
        calculation.Lines.Select(x => new QuoteCustomSewingLineResponse(x.Kind.ToString(), x.ReferenceId, x.Name,
            x.Unit, x.UnitValue, x.IsActive, x.CostCode)).ToArray(),
        calculation.Variants.Select(x => new QuoteCustomSewingVariantResponse(x.Title,
            x.Fabrics.Select(f => new QuoteCustomSewingFabricResponse(f.FabricId, f.Meters, f.Name, f.Details,
                f.PricePerMeter)).ToArray(),
            x.Minutes, x.LaborCost, x.FullCost,
            x.Prices is { } prices ? new QuoteTierPricesResponse(prices.Retail, prices.Wholesale, prices.Large) : null,
            x.Problems)).ToArray());
}
