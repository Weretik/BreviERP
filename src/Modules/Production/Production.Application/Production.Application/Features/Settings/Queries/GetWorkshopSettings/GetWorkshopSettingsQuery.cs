using Production.Application.Abstractions;
using Production.Domain.Settings;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Settings.Shared;

namespace Production.Application.Features.Settings.Queries.GetWorkshopSettings;

/// <summary>«Налаштування цеху» with the groups the bot is in.</summary>
public sealed record GetWorkshopSettingsQuery(ProductionActor Actor) : IQuery<Result<WorkshopSettingsResponse>>;
