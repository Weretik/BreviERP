using Production.Application.Features.Orders.Queries.GetProductionEmbroideries;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Items;

namespace Production.Application.Abstractions.Sales;

/// <summary>
/// Saved «Ціна вишивки» calculations of Crm for the «Вишивка» of an item; implemented in the host over the Crm
/// embroidery queries. Nobody gets costs or prices here.
/// </summary>
public interface IProductionEmbroideryReader
{
    /// <summary>Newest first; the search matches the name, the client or «ВШ-0042».</summary>
    Task<IReadOnlyList<ProductionEmbroideryChoiceResponse>> SearchAsync(int userId, string? search,
        CancellationToken cancellationToken);

    /// <returns>The calculations that still exist; a removed one is left out.</returns>
    Task<IReadOnlyDictionary<long, ProductionEmbroideryResponse>> ReadAsync(int userId, IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken);
}
