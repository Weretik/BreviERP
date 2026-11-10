using Production.Application.Contracts;

namespace Production.Application.FabricNorms.GetCompositions;

/// <summary>Every Catalog sewing model with its extra layers and full composition, by model code.</summary>
public sealed record GetModelCompositionsQuery : IQuery<Result<IReadOnlyList<ModelCompositionResponse>>>;
