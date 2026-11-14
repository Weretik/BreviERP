using Production.Application.Features.Colors.Shared;
namespace Production.Application.Features.Colors.Queries.GetProductionColors;

/// <summary>Colours of «Кольори» for the item form, so the workshop does not need access to «Довідники».</summary>
public sealed record GetProductionColorsQuery : IQuery<Result<IReadOnlyList<string>>>;
