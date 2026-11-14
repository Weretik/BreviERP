using Production.Application.Abstractions;
using Production.Domain.Settings;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Settings.Queries.GetWorkshopSettings;

public sealed class GetWorkshopSettingsValidator : AbstractValidator<GetWorkshopSettingsQuery>
{
    public GetWorkshopSettingsValidator() => RuleFor(x => x.Actor).ValidActor();
}
