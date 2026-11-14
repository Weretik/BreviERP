using Production.Application.Features.Colors.Shared;
namespace Production.Application.Features.Colors.Queries.GetProductionColors;

/// <summary>Colours of «Кольори» for the item form, so the workshop does not need access to «Довідники».</summary>
public sealed record GetProductionColorsQuery : IQuery<Result<IReadOnlyList<string>>>;

public sealed class GetProductionColorsHandler(ProductionColors colors)
    : IQueryHandler<GetProductionColorsQuery, Result<IReadOnlyList<string>>>
{
    public async ValueTask<Result<IReadOnlyList<string>>> Handle(GetProductionColorsQuery query,
        CancellationToken cancellationToken) =>
        Result.Success(await colors.ListAsync(cancellationToken));
}
