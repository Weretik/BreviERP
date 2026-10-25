using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Sewing.Settings.CreateVersion;

public sealed class CreateSewingSettingsVersionCommandHandler(ISewingPricingSettingsWriter writer)
    : ICommandHandler<CreateSewingSettingsVersionCommand, Result<SewingPricingSettingsSelection>>
{
    public async ValueTask<Result<SewingPricingSettingsSelection>> Handle(
        CreateSewingSettingsVersionCommand command, CancellationToken cancellationToken)
    {
        if (command.ExpectedVersionId <= 0 || command.RequestId == Guid.Empty || command.Settings is null
            || string.IsNullOrWhiteSpace(command.Actor) || command.Actor.Length > 200)
            return Result.Invalid(new ValidationError("request", "Current version, settings, request UUID and actor are required."));
        var blockers = SewingSettingsValidation.Validate(command.Settings);
        if (blockers.Count != 0)
            return Result.Invalid(blockers.Select(x => new ValidationError(x.InputKey ?? "settings", x.Message)));
        return await writer.AppendAsync(command.ExpectedVersionId, command.RequestId, command.Settings, command.Actor, cancellationToken);
    }
}
