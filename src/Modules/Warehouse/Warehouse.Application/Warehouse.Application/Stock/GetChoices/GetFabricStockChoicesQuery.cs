using Warehouse.Application.Abstractions;
using Warehouse.Application.Colors;
using Warehouse.Application.Contracts;
using Warehouse.Application.Access;

namespace Warehouse.Application.Stock.GetChoices;

/// <summary>Fabrics with a FAB- code, active suppliers and the colours of «Кольори» for the stock forms.</summary>
public sealed record GetFabricStockChoicesQuery(WarehouseActor Actor) : IQuery<Result<FabricStockChoicesResponse>>;

public sealed class GetFabricStockChoicesQueryValidator : AbstractValidator<GetFabricStockChoicesQuery>
{
    public GetFabricStockChoicesQueryValidator() => RuleFor(x => x.Actor).ValidActor();
}

public sealed class GetFabricStockChoicesQueryHandler(IWarehouseReferenceReader reference, WarehouseColors colors) : IQueryHandler<GetFabricStockChoicesQuery, Result<FabricStockChoicesResponse>>
{
    public async ValueTask<Result<FabricStockChoicesResponse>> Handle(GetFabricStockChoicesQuery query,
        CancellationToken cancellationToken)
    {
        var fabricList = (await reference.FabricsAsync(cancellationToken))
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .Select(x => new FabricStockFabricChoice(x.Id, x.Code!.Trim().ToUpperInvariant(), x.Name))
            .OrderBy(x => x.Code, StringComparer.Ordinal).ToList();
        var supplierList = query.Actor.CanMoveStock
            ? (await reference.SuppliersAsync(cancellationToken)).Select(x => new FabricStockSupplierChoice(x.Id, x.Name))
                .OrderBy(x => x.Name, StringComparer.CurrentCulture).ToList()
            : [];
        return Result.Success(new FabricStockChoicesResponse(fabricList, supplierList,
            await colors.ListAsync(cancellationToken)));
    }
}
