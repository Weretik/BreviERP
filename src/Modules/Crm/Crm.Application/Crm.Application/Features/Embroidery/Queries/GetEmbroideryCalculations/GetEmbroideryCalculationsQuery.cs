using Crm.Domain.Embroidery.Saved;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculations;

/// <summary>«Збережені»: newest first; the search matches the name, the client or the number «ВШ-0042».</summary>
public sealed record GetEmbroideryCalculationsQuery(EmbroideryActor Actor, string? Search, int Page = 1,
    int PageSize = 25) : IQuery<Result<EmbroiderySavedPageResponse>>;
