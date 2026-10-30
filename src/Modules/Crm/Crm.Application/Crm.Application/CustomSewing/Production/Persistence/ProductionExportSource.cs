using Crm.Application.CustomSewing.Production.Projection;
using Crm.Domain.Quotes.Versions;
namespace Crm.Application.CustomSewing.Production.Persistence;
public sealed record ProductionExportSource(Guid QuotePublicId, string QuoteNumber,
    QuoteVersion Version, IReadOnlyList<ProductionCustomVersionSource> CustomVersions);
