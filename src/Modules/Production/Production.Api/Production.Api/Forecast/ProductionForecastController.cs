using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Features.Forecast.Commands.SaveForecastQueue;
using Production.Application.Features.Forecast.Queries.GetForecast;

namespace Production.Api.Forecast;

/// <summary>«Прогноз пошиття»: everyone with production orders sees it; the workshop chief and Admin reorder it.</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.Forecast)]
[Route("api/production/forecast")]
public sealed class ProductionForecastController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getSewingForecast")]
    public async Task<ActionResult<ForecastResponse>> Get(CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetForecastQuery(actor), cancellationToken))
            : Forbid();

    [HttpPut("queue", Name = "saveSewingForecastQueue")]
    public async Task<ActionResult> SaveQueue(SaveForecastQueueInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToNoContentResult(await sender.Send(new SaveForecastQueueCommand(actor, input), cancellationToken))
            : Forbid();
}
