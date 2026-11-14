using Catalog.Domain.Pricing.Sewing;
using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Application.Features.Pricing.Commands.CreateSewingSettingsVersion;

public sealed record CreateSewingSettingsVersionCommand(
    long ExpectedVersionId, Guid RequestId, SewingCalculationSettings Settings, string Actor)
    : ICommand<Result<SewingPricingSettingsSelection>>;
