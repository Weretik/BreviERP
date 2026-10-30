using Crm.Application.Quotes.Pricing;
using Crm.Domain.CustomSewing.Identifiers;

namespace Crm.Application.CustomSewing.Production.Projection;

/// <summary>The source reader must resolve this exact immutable CustomVersionId and its pinned files.</summary>
public sealed record ProductionCustomVersionSource(CustomVersionId Id,
    QuoteCustomVersionSnapshot Snapshot, IReadOnlyList<ProductionPinnedFile> Files);
