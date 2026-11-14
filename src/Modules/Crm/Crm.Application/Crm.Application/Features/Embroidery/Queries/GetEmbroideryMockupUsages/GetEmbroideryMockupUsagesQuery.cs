using Crm.Domain.Embroidery.Saved;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryMockupUsages;

/// <summary>Saved calculations whose «Макет» is one of the photos, so the Media library does not delete it.</summary>
public sealed record GetEmbroideryMockupUsagesQuery(IReadOnlyCollection<int> MediaFileIds)
    : IQuery<Result<IReadOnlyList<EmbroideryMockupUsage>>>;
