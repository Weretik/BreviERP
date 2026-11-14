using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Orders.Queries.GetProductionEmbroideries;

/// <summary>«Розрахунок вишивки» in the order form: saved calculations to pick, newest first.</summary>
public sealed record GetProductionEmbroideriesQuery(ProductionActor Actor, string? Search)
    : IQuery<Result<IReadOnlyList<ProductionEmbroideryChoiceResponse>>>;

public sealed class GetProductionEmbroideriesValidator : AbstractValidator<GetProductionEmbroideriesQuery>
{
    public GetProductionEmbroideriesValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Search).MaximumLength(ItemApplication.TextMaxLength);
    }
}

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
