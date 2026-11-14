using Production.Application.Abstractions;
using Production.Domain.Settings;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Settings.Shared;

namespace Production.Application.Features.Settings.Commands.UpdateWorkshopSettings;

public sealed record UpdateWorkshopSettingsCommand(ProductionActor Actor, WorkshopSettingsInput Input)
    : ICommand<Result<WorkshopSettingsResponse>>;
