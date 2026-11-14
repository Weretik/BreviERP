using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Enums;

namespace Crm.Application.Abstractions.Quotes;

/// <summary>
/// The composition of a Catalog sewing model, the start of «Змінити склад» (R-21). Implemented in the host over the
/// Catalog queries; Crm never reads their tables.
/// </summary>
public interface IQuoteModelCompositionReader
{
    /// <returns>Null when the product is not a sewing model.</returns>
    Task<QuoteModelComposition?> ReadAsync(int productId, CancellationToken cancellationToken);
}

/// <param name="Fabrics">The fabric of each variant with its metres.</param>
/// <param name="Lines">Shared accessories, services, extra fabrics and operations, in the Catalog order.</param>
public sealed record QuoteModelComposition(
    int ProductId, IReadOnlyList<QuoteModelFabric> Fabrics, IReadOnlyList<QuoteModelLine> Lines);

public sealed record QuoteModelFabric(long ArticleId, int FabricId, string? Name, decimal Meters, bool IsActive);

/// <param name="Name">The reference name, or the code of a service; null when the reference row is gone.</param>
public sealed record QuoteModelLine(
    QuoteItemLineKind Kind, int ReferenceId, string? CostCode, string? Name, string Unit, decimal Quantity,
    decimal? ManualMinutes);
