using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Settings.GetCurrent;

public sealed record GetSewingSettingsQuery : IQuery<Result<SewingPricingSettingsSelection>>;
