using Crm.Application.Quotes.Catalog;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.CustomSewing;

/// <summary>
/// Prices a «Виріб з нуля» by the Catalog sewing formula, once per fabric. Implemented in the host over the Catalog
/// and Reference queries; Crm never reads their tables.
/// </summary>
public interface IQuoteSewingCalculator
{
    Task<QuoteSewingCalculation> CalculateAsync(QuoteSewingComposition composition, CancellationToken cancellationToken);
}

/// <summary>The fabrics to price and the accessories and operations they share.</summary>
public sealed record QuoteSewingComposition(IReadOnlyList<QuoteSewingFabric> Fabrics, IReadOnlyList<QuoteSewingLine> Lines);

public sealed record QuoteSewingFabric(int FabricId, decimal Meters);

public sealed record QuoteSewingLine(QuoteItemLineKind Kind, int ReferenceId, decimal Quantity, decimal? ManualMinutes);

/// <summary>Reference data of each line and the price of each fabric, in the order given.</summary>
public sealed record QuoteSewingCalculation(
    IReadOnlyList<QuoteSewingLineInfo> Lines, IReadOnlyList<QuoteSewingFabricPrice> Fabrics);

/// <param name="Name">Null when the reference row is gone.</param>
/// <param name="UnitValue">Accessory price per unit or operation norm in minutes.</param>
public sealed record QuoteSewingLineInfo(
    QuoteItemLineKind Kind, int ReferenceId, string? Name, string? Unit, decimal? UnitValue, bool IsActive);

/// <param name="Name">Fabric name; null when the fabric is gone.</param>
/// <param name="Details">The client text of the fabric (composition, density, КП description).</param>
/// <param name="LaborCost">Minutes × cost of a minute, UAH.</param>
/// <param name="Problems">Why <paramref name="Prices"/> is null, in the manager's words.</param>
public sealed record QuoteSewingFabricPrice(
    int FabricId,
    decimal Meters,
    string? Name,
    string? Details,
    decimal? PricePerMeter,
    decimal? Minutes,
    decimal? LaborCost,
    decimal? FullCost,
    QuoteTierPrices? Prices,
    IReadOnlyList<string> Problems);
