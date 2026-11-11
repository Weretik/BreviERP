using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Colors;

/// <summary>
/// The stock picks colours only from «Кольори» (REF-006). A colour already on a stock line stays valid, so a line
/// made before the list keeps working.
/// </summary>
public sealed class WarehouseColors(IWarehouseReferenceReader reference)
{
    public const string NotInList = "Колір «{0}» не знайдено в довіднику «Кольори». Оберіть колір зі списку.";

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken) =>
        reference.ColorsAsync(cancellationToken);

    /// <summary>An error for each colour that is not active in «Кольори».</summary>
    public async Task<IReadOnlyList<ValidationError>> CheckAsync(IEnumerable<string?> values,
        CancellationToken cancellationToken)
    {
        var allowed = (await reference.ColorsAsync(cancellationToken)).Select(StockColor.Key)
            .ToHashSet(StringComparer.Ordinal);
        return values.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Where(x => !allowed.Contains(StockColor.Key(x)))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .Select(x => new ValidationError("Color", string.Format(NotInList, x)))
            .ToArray();
    }
}
