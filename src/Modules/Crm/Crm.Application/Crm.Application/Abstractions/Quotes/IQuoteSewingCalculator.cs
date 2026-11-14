using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared.Catalog;

namespace Crm.Application.Abstractions.Quotes;

/// <summary>
/// Prices a «Виріб з нуля» by the Catalog sewing formula, once per variant: the variant's fabrics are summed with the
/// shared accessories and operations (R-26). Implemented in the host over the Catalog and Reference queries; Crm never
/// reads their tables.
/// </summary>
public interface IQuoteSewingCalculator
{
    Task<QuoteSewingCalculation> CalculateAsync(QuoteSewingComposition composition, CancellationToken cancellationToken);
}

/// <summary>The variants to price and the accessories, operations, services and extra fabrics they share.</summary>
public sealed record QuoteSewingComposition(IReadOnlyList<QuoteSewingVariant> Variants, IReadOnlyList<QuoteSewingLine> Lines);

/// <summary>One variant: one or more fabrics, each with its metres in one product.</summary>
public sealed record QuoteSewingVariant(IReadOnlyList<QuoteSewingFabric> Fabrics)
{
    public static QuoteSewingVariant Of(int fabricId, decimal meters) => new([new QuoteSewingFabric(fabricId, meters)]);
}

public sealed record QuoteSewingFabric(int FabricId, decimal Meters);

/// <param name="CostCode">The other-cost code of a service; its <paramref name="ReferenceId"/> is 0.</param>
public sealed record QuoteSewingLine(
    QuoteItemLineKind Kind, int ReferenceId, decimal Quantity, decimal? ManualMinutes, string? CostCode = null);

/// <summary>Reference data of each line and the price of each variant, in the order given.</summary>
public sealed record QuoteSewingCalculation(
    IReadOnlyList<QuoteSewingLineInfo> Lines, IReadOnlyList<QuoteSewingVariantPrice> Variants);

/// <param name="Name">Null when the reference row is gone.</param>
/// <param name="UnitValue">Accessory, fabric or service price per unit, or operation norm in minutes.</param>
public sealed record QuoteSewingLineInfo(
    QuoteItemLineKind Kind, int ReferenceId, string? Name, string? Unit, decimal? UnitValue, bool IsActive,
    string? CostCode = null);

/// <param name="Name">Fabric name; null when the fabric is gone.</param>
/// <param name="Details">The client text of the fabric (composition, density, КП description).</param>
public sealed record QuoteSewingFabricInfo(
    int FabricId, decimal Meters, string? Name, string? Details, decimal? PricePerMeter);

/// <param name="LaborCost">Minutes × cost of a minute, UAH.</param>
/// <param name="Problems">Why <paramref name="Prices"/> is null, in the manager's words.</param>
public sealed record QuoteSewingVariantPrice(
    IReadOnlyList<QuoteSewingFabricInfo> Fabrics,
    decimal? Minutes,
    decimal? LaborCost,
    decimal? FullCost,
    QuoteTierPrices? Prices,
    IReadOnlyList<string> Problems)
{
    /// <summary>«Грета + Сітка»: the names of the variant's fabrics; null when one of them is gone.</summary>
    public string? Title => Fabrics.Any(x => x.Name is null) ? null : string.Join(" + ", Fabrics.Select(x => x.Name));

    /// <summary>The client text: of the only fabric, or «Name: text» of each fabric.</summary>
    public string? Details
    {
        get
        {
            if (Fabrics.Count == 1) return Fabrics[0].Details;
            var parts = Fabrics.Where(x => !string.IsNullOrWhiteSpace(x.Details)).Select(x => $"{x.Name}: {x.Details}")
                .ToArray();
            return parts.Length == 0 ? null : string.Join("; ", parts);
        }
    }
}
