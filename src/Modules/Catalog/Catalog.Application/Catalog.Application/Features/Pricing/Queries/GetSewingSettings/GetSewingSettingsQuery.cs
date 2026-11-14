using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Application.Features.Pricing.Queries.GetSewingSettings;

public sealed record GetSewingSettingsQuery : IQuery<Result<SewingPricingSettingsSelection>>;
