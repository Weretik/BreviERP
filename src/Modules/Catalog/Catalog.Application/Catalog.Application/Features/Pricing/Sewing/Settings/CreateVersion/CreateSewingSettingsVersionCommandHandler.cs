using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Sewing.Settings.CreateVersion;

public sealed class CreateSewingSettingsVersionCommandHandler(ISewingPricingSettingsWriter writer)
    : ICommandHandler<CreateSewingSettingsVersionCommand, Result<SewingPricingSettingsSelection>>
{
    public async ValueTask<Result<SewingPricingSettingsSelection>> Handle(
        CreateSewingSettingsVersionCommand command, CancellationToken cancellationToken)
    {
        var blockers = SewingSettingsValidation.Validate(command.Settings);
        if (blockers.Count != 0)
            return Result.Invalid(blockers.Select(x => new ValidationError(x.InputKey ?? "settings", x.Message)));
        return await writer.AppendAsync(command.ExpectedVersionId, command.RequestId, command.Settings, command.Actor, cancellationToken);
    }
}
