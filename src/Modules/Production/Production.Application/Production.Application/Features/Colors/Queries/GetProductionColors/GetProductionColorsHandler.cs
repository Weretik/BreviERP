using Production.Application.Features.Colors.Shared;
namespace Production.Application.Features.Colors.Queries.GetProductionColors;

public sealed class GetProductionColorsHandler(ProductionColors colors)
    : IQueryHandler<GetProductionColorsQuery, Result<IReadOnlyList<string>>>
{
    public async ValueTask<Result<IReadOnlyList<string>>> Handle(GetProductionColorsQuery query,
        CancellationToken cancellationToken) =>
        Result.Success(await colors.ListAsync(cancellationToken));
}
