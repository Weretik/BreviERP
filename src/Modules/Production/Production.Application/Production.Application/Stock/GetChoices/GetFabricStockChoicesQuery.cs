using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;

namespace Production.Application.Stock.GetChoices;

/// <summary>Fabrics with a FAB- code and active suppliers for the stock forms.</summary>
public sealed record GetFabricStockChoicesQuery(ProductionActor Actor) : IQuery<Result<FabricStockChoicesResponse>>;

public sealed class GetFabricStockChoicesQueryValidator : AbstractValidator<GetFabricStockChoicesQuery>
{
    public GetFabricStockChoicesQueryValidator() => RuleFor(x => x.Actor).ValidActor();
}

public sealed class GetFabricStockChoicesQueryHandler(IProductionFabricReader fabrics,
    IProductionSupplierReader suppliers) : IQueryHandler<GetFabricStockChoicesQuery, Result<FabricStockChoicesResponse>>
{
    public async ValueTask<Result<FabricStockChoicesResponse>> Handle(GetFabricStockChoicesQuery query,
        CancellationToken cancellationToken)
    {
        var fabricList = (await fabrics.ListAsync(cancellationToken))
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .Select(x => new FabricStockFabricChoice(x.Id, x.Code!.Trim().ToUpperInvariant(), x.Name))
            .OrderBy(x => x.Code, StringComparer.Ordinal).ToList();
        var supplierList = query.Actor.CanMoveStock
            ? (await suppliers.ListAsync(cancellationToken)).Select(x => new FabricStockSupplierChoice(x.Id, x.Name))
                .OrderBy(x => x.Name, StringComparer.CurrentCulture).ToList()
            : [];
        return Result.Success(new FabricStockChoicesResponse(fabricList, supplierList));
    }
}
