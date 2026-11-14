
namespace Production.Application.Features.FabricNorms.Queries.GetModelCompositions;

/// <summary>Every Catalog sewing model with its extra layers and full composition, by model code.</summary>
public sealed record GetModelCompositionsQuery : IQuery<Result<IReadOnlyList<ModelCompositionResponse>>>;
