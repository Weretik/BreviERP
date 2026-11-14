using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Application.Features.Pricing.Commands.CreateSewingSettingsVersion;

public sealed record CreateSewingSettingsVersionCommand(
    long ExpectedVersionId, Guid RequestId, SewingSettingsValues Settings, string Actor)
    : ICommand<Result<SewingPricingSettingsSelection>>;
