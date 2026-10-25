namespace Catalog.Application.Contracts.Pricing;

public sealed record SavedSewingCalculation(
    Guid Id, Guid RequestId, DateTimeOffset CreatedAt, string CreatedBy, SewingArticlePricePreview Snapshot);

