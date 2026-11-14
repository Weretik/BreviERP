using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricStockChoices;

public sealed class GetFabricStockChoicesHandler(IWarehouseReferenceReader reference, WarehouseColors colors)
    : IQueryHandler<GetFabricStockChoicesQuery, Result<FabricStockChoicesResponse>>
{
    public async ValueTask<Result<FabricStockChoicesResponse>> Handle(GetFabricStockChoicesQuery query,
        CancellationToken cancellationToken)
    {
        var fabrics = (await reference.FabricsAsync(cancellationToken))
            .Where(x => x.IsActive && !string.IsNullOrWhiteSpace(x.Code) &&
                        FabricAliases.MatchesGroup(x.Code, x.Composition, x.Density))
            .ToList();
        var fabricList = fabrics
            .Select(x => new FabricStockFabricChoice(x.Id, FabricAliases.Normalize(x.Code!), FabricAliases.Canonical(x.Code!),
                x.Name, x.Composition, x.Density, x.Supplier, query.Actor.SeesStockValue ? x.Price : null))
            .OrderBy(x => x.GroupCode, StringComparer.Ordinal).ThenBy(x => x.Code, StringComparer.Ordinal).ToList();
        var suppliers = query.Actor.CanMoveStock
            ? (await reference.SuppliersAsync(cancellationToken)).Select(x => x.Name)
                .Concat(fabrics.Select(x => x.Supplier ?? string.Empty))
                .Select(x => x.Trim()).Where(x => x.Length > 0)
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .OrderBy(x => x, StringComparer.CurrentCulture).ToList()
            : [];
        return Result.Success(new FabricStockChoicesResponse(fabricList, suppliers,
            await colors.ListAsync(cancellationToken)));
    }
}
