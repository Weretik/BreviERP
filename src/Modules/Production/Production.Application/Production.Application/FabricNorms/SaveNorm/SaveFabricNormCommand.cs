using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain.FabricNorms;

namespace Production.Application.FabricNorms.SaveNorm;

/// <summary>Sets the norm of a Catalog sewing model; creates it on the first save.</summary>
public sealed record SaveFabricNormCommand(string ModelCode, FabricNormInput Norm, int? UserId)
    : ICommand<Result<FabricNormRowResponse>>;
