using Crm.Application.Quotes.Contracts;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.CustomSewing;

/// <summary>The composition the «Виріб з нуля» panel prices before it goes into the КП.</summary>
public sealed record QuoteCustomSewingPriceInput(
    IReadOnlyList<QuoteCustomSewingFabricInput> Fabrics, IReadOnlyList<QuoteItemLineInput> Lines);

public sealed record QuoteCustomSewingFabricInput(int FabricId, decimal Meters);

public sealed record QuoteCustomSewingPriceResponse(
    IReadOnlyList<QuoteCustomSewingLineResponse> Lines, IReadOnlyList<QuoteCustomSewingFabricResponse> Fabrics);

public sealed record QuoteCustomSewingLineResponse(
    string Kind, int ReferenceId, string? Name, string? Unit, decimal? UnitValue, bool IsActive, string? CostCode);

public sealed record QuoteCustomSewingFabricResponse(
    int FabricId,
    decimal Meters,
    string? Name,
    string? Details,
    decimal? PricePerMeter,
    decimal? Minutes,
    decimal? LaborCost,
    decimal? FullCost,
    QuoteTierPricesResponse? Prices,
    IReadOnlyList<string> Problems);

internal static class QuoteCustomSewingMapper
{
    public static QuoteSewingLine ToLine(this QuoteItemLineInput input) =>
        new(Enum.Parse<QuoteItemLineKind>(input.Kind), input.ReferenceId, input.Quantity, input.ManualMinutes,
            input.CostCode?.Trim());

    public static QuoteCustomSewingPriceResponse ToResponse(this QuoteSewingCalculation calculation) => new(
        calculation.Lines.Select(x => new QuoteCustomSewingLineResponse(x.Kind.ToString(), x.ReferenceId, x.Name,
            x.Unit, x.UnitValue, x.IsActive, x.CostCode)).ToArray(),
        calculation.Fabrics.Select(x => new QuoteCustomSewingFabricResponse(x.FabricId, x.Meters, x.Name, x.Details,
            x.PricePerMeter, x.Minutes, x.LaborCost, x.FullCost,
            x.Prices is { } prices ? new QuoteTierPricesResponse(prices.Retail, prices.Wholesale, prices.Large) : null,
            x.Problems)).ToArray());
}
