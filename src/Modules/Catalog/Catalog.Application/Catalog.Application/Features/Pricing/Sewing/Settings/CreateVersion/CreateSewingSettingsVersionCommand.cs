using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Sewing.Settings.CreateVersion;

public sealed record CreateSewingSettingsVersionCommand(
    long ExpectedVersionId, Guid RequestId, SewingCalculationSettings Settings, string Actor)
    : ICommand<Result<SewingPricingSettingsSelection>>;
