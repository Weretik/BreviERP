using Production.Application.Abstractions;
using Production.Application.Features.FabricNorms.Shared;

namespace Production.Application.Features.FabricNorms.Queries.GetFabricNorms;

/// <summary>
/// Every Catalog sewing model with its norm (empty when not measured yet), then norms whose model left Catalog. The
/// list is small (about 130 models), so the Admin searches and filters it on the page.
/// </summary>
public sealed record GetFabricNormsQuery : IQuery<Result<IReadOnlyList<FabricNormRowResponse>>>;
