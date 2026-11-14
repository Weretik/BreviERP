using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Domain.Orders.Enums;

namespace Production.Application.Features.Orders.Queries.GetProductionEmbroideries;

/// <summary>
/// The saved «Ціна вишивки» calculations an order's embroideries point to (PRD-D41): checked when the order is saved
/// and read again, as they are now, for the order card.
/// </summary>
public sealed class EmbroideryLinks(IProductionEmbroideryReader reader)
{
    /// <param name="order">When editing: links it already has stay, even if their calculation was removed.</param>
    /// <returns>The «ВШ-…» number of every linked calculation, or an error when a new link does not exist.</returns>
    public async Task<Result<IReadOnlyDictionary<long, string>>> NumbersAsync(int userId,
        IEnumerable<ProductionItemDetailsInput> details, ProductionOrder? order, CancellationToken cancellationToken)
    {
        var numbers = new Dictionary<long, string>();
        foreach (var kept in order?.Items.SelectMany(x => x.Applications) ?? [])
            if (kept is { EmbroideryCalculationId: { } keptId, EmbroideryNumber: { } keptNumber })
                numbers[keptId] = keptNumber;

        var ids = details.SelectMany(x => x.Applications ?? [])
            .Where(x => IsEmbroidery(x.Kind) && x.EmbroideryCalculationId is > 0)
            .Select(x => x.EmbroideryCalculationId!.Value).Distinct().Where(x => !numbers.ContainsKey(x)).ToArray();
        if (ids.Length == 0) return Result.Success<IReadOnlyDictionary<long, string>>(numbers);

        var found = await reader.ReadAsync(userId, ids, cancellationToken);
        if (!ids.All(found.ContainsKey)) return Result.Invalid(new ValidationError(OrderMessages.EmbroideryNotFound));
        foreach (var id in ids) numbers[id] = found[id].Number;
        return Result.Success<IReadOnlyDictionary<long, string>>(numbers);
    }

    public async Task<IReadOnlyDictionary<long, ProductionEmbroideryResponse>> ReadAsync(ProductionOrder order,
        int userId, CancellationToken cancellationToken)
    {
        var ids = order.Items.SelectMany(x => x.Applications)
            .Select(x => x.EmbroideryCalculationId).OfType<long>().Distinct().ToArray();
        return ids.Length == 0
            ? new Dictionary<long, ProductionEmbroideryResponse>()
            : await reader.ReadAsync(userId, ids, cancellationToken);
    }

    private static bool IsEmbroidery(string kind) =>
        Enum.TryParse<ItemApplicationKind>(kind, true, out var parsed) && parsed == ItemApplicationKind.Embroidery;
}
