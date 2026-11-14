using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Abstractions.Sales;

namespace Production.Application.Features.Orders.Queries.GetProductionEmbroideries;

public sealed class GetProductionEmbroideriesHandler(IProductionEmbroideryReader reader)
    : IQueryHandler<GetProductionEmbroideriesQuery, Result<IReadOnlyList<ProductionEmbroideryChoiceResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ProductionEmbroideryChoiceResponse>>> Handle(
        GetProductionEmbroideriesQuery query, CancellationToken cancellationToken)
    {
        var actor = query.Actor;
        if (!actor.CanCreate && !actor.IsWorkshopChief)
            return Result<IReadOnlyList<ProductionEmbroideryChoiceResponse>>.Forbidden(OrderMessages.CannotEdit);
        return Result.Success(await reader.SearchAsync(actor.UserId,
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(), cancellationToken));
    }
}
