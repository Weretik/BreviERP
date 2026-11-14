using Crm.Contracts.Embroidery;
using Production.Application.Abstractions;
using Production.Application.Features.Orders.Queries.GetProductionEmbroideries;
using Production.Application.Features.Orders.Shared;

namespace Production.Infrastructure.Adapters.Crm;

/// <summary>Saved «Ціна вишивки» calculations of Crm for the workshop, read without costs.</summary>
internal sealed class ProductionEmbroideryReader(IEmbroideryCalculationReader calculations) : IProductionEmbroideryReader
{
    private const int MaxChoices = 50;

    public async Task<IReadOnlyList<ProductionEmbroideryChoiceResponse>> SearchAsync(int userId, string? search,
        CancellationToken cancellationToken) =>
        (await calculations.SearchAsync(userId, search, MaxChoices, cancellationToken))
        .Select(x => new ProductionEmbroideryChoiceResponse(x.Id, x.Number, x.Name, x.CustomerName, x.Quantity,
            x.StitchesPerUnit, x.HoopLabel, x.MockupThumbnailUrl)).ToArray();

    public async Task<IReadOnlyDictionary<long, ProductionEmbroideryResponse>> ReadAsync(int userId,
        IReadOnlyCollection<long> ids, CancellationToken cancellationToken)
    {
        var found = new Dictionary<long, ProductionEmbroideryResponse>();
        foreach (var id in ids.Distinct())
        {
            if (await calculations.FindAsync(userId, id, cancellationToken) is not { } x) continue;
            found[id] = new ProductionEmbroideryResponse(x.Id, x.Number, x.Name, x.CustomerName, x.Mode, x.Quantity,
                x.StitchesPerUnit, x.HoopLabel, x.Hoopings, x.StabilizerMetres, x.FabricMetres, x.MockupUrl,
                x.MockupThumbnailUrl);
        }

        return found;
    }
}
