using Crm.Domain.Embroidery.Saved;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryCalculations;

/// <summary>«Збережені»: newest first; the search matches the name, the client or the number «ВШ-0042».</summary>
/// <param name="CustomerPublicId">The client card: only this client's calculations (CARD-001 R-05).</param>
/// <param name="From">First Kyiv day of saving, inclusive.</param>
/// <param name="To">Last Kyiv day of saving, inclusive.</param>
public sealed record GetEmbroideryCalculationsQuery(EmbroideryActor Actor, string? Search, int Page = 1,
    int PageSize = 25, Guid? CustomerPublicId = null, DateOnly? From = null, DateOnly? To = null)
    : IQuery<Result<EmbroiderySavedPageResponse>>;
