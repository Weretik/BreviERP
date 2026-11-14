using Crm.Domain.Embroidery.Saved;

namespace Crm.Application.Features.Embroidery.Queries.GetEmbroideryMockupUsages;

/// <param name="Title">«ВШ-0042 Логотип».</param>
public sealed record EmbroideryMockupUsage(int MediaFileId, long CalculationId, string Title);
