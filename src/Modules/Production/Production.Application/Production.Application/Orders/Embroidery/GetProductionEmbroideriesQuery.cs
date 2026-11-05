using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Domain.Orders;

namespace Production.Application.Orders.Embroidery;

/// <summary>«Розрахунок вишивки» in the order form: saved calculations to pick, newest first.</summary>
public sealed record GetProductionEmbroideriesQuery(ProductionActor Actor, string? Search)
    : IQuery<Result<IReadOnlyList<ProductionEmbroideryChoiceResponse>>>;

public sealed class GetProductionEmbroideriesQueryValidator : AbstractValidator<GetProductionEmbroideriesQuery>
{
    public GetProductionEmbroideriesQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Search).MaximumLength(ItemApplication.TextMaxLength);
    }
}

public sealed class GetProductionEmbroideriesQueryHandler(IProductionEmbroideryReader reader)
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
