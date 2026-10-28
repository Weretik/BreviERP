using Crm.Domain.CustomSewing.Identifiers;

namespace Crm.Application.Quotes.Pricing;

public sealed record ResolvedCustomQuoteVersion(CustomVersionId Id, QuoteCustomVersionSnapshot Snapshot);
