using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Settings;

namespace Catalog.Application.Features.Pricing.Queries.GetSewingSettings;

public sealed record GetSewingSettingsQuery : IQuery<Result<SewingPricingSettingsSelection>>;
