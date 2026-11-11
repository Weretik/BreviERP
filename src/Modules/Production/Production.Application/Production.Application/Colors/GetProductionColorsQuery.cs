namespace Production.Application.Colors;

/// <summary>Colours of «Кольори» for the item form, so the workshop does not need access to «Довідники».</summary>
public sealed record GetProductionColorsQuery : IQuery<Result<IReadOnlyList<string>>>;

public sealed class GetProductionColorsQueryHandler(ProductionColors colors)
    : IQueryHandler<GetProductionColorsQuery, Result<IReadOnlyList<string>>>
{
    public async ValueTask<Result<IReadOnlyList<string>>> Handle(GetProductionColorsQuery query,
        CancellationToken cancellationToken) =>
        Result.Success(await colors.ListAsync(cancellationToken));
}
