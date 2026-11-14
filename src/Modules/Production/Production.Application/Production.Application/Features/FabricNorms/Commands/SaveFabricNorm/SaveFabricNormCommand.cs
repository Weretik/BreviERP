using Production.Application.Abstractions;
using Production.Domain.FabricNorms;
using Production.Application.Features.FabricNorms.Shared;

namespace Production.Application.Features.FabricNorms.Commands.SaveFabricNorm;

/// <summary>Sets the norm of a Catalog sewing model; creates it on the first save.</summary>
public sealed record SaveFabricNormCommand(string ModelCode, FabricNormInput Norm, int? UserId)
    : ICommand<Result<FabricNormRowResponse>>;
