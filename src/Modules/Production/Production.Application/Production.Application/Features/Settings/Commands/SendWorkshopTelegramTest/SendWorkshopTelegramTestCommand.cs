using Production.Application.Abstractions;
using Production.Domain.Settings;
using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Settings.Commands.SendWorkshopTelegramTest;

/// <summary>Sends a test line to the chosen group and topic.</summary>
public sealed record SendWorkshopTelegramTestCommand(ProductionActor Actor) : ICommand<Result>;
