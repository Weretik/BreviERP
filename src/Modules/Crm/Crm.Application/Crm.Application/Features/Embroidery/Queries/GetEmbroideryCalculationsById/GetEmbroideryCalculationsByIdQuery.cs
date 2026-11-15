using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculationsById;

/// <summary>Several saved calculations at once, for another module; an id that is not found is left out.</summary>
public sealed record GetEmbroideryCalculationsByIdQuery(EmbroideryActor Actor, IReadOnlyCollection<long> Ids)
    : IQuery<Result<IReadOnlyList<EmbroiderySavedCalculationResponse>>>
{
    public const int MaxIds = 500;
}
