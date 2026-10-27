namespace Crm.Application.Quotes.Pricing;

public sealed record QuoteCatalogPrice(
    string Kind,
    int ProductId,
    long? ArticleId,
    Guid SourceId,
    Guid? CalculationId,
    Guid? RuleVersionId,
    long? CompositionVersion,
    long? SettingsVersionId,
    IReadOnlyList<long> ReferenceVersionIds,
    string FormulaVersion,
    decimal? Retail,
    decimal? Wholesale,
    decimal? LargeWholesale,
    IReadOnlyList<string> Blockers);
