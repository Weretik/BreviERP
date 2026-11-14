using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Access;
using Warehouse.Application.Features.Stock.Queries.GetFabricBooking;
using Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact;
using Warehouse.Application.Features.Stock.Queries.GetFabricValuation;
using Warehouse.Application.Features.Stock.Queries.GetHardwareOrder;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Stock;

/// <summary>
/// The four reports of «Склад тканин»: «План і факт», «Бронь і що замовити», «Вартість складу» and «Фурнітура до
/// замовлення». Each is a page of «Права доступу»; each reads only and is calculated anew on every request.
/// </summary>
[ApiController]
[Route("api/warehouse/reports")]
public sealed class StockReportsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// «План і факт»: main fabric by model; filters match a part of the model, client or fabric; the period keeps the
    /// items whose last issue is in it.
    /// </summary>
    [HttpGet("plan-fact", Name = "getFabricPlanFact")]
    [Authorize(Policy = WarehouseAccess.FabricPlanFact)]
    public async Task<ActionResult<FabricPlanFactResponse>> PlanFact([FromQuery] string? model = null,
        [FromQuery] string? client = null, [FromQuery] string? fabric = null, [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null, CancellationToken cancellationToken = default) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(
                new GetFabricPlanFactQuery(actor, model, client, fabric, from, to), cancellationToken))
            : Forbid();

    /// <summary>«Бронь і що замовити»: the stock handed to the plans of open orders in order; what to order.</summary>
    [HttpGet("booking", Name = "getFabricBooking")]
    [Authorize(Policy = WarehouseAccess.FabricBooking)]
    public async Task<ActionResult<FabricBookingResponse>> Booking(CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetFabricBookingQuery(actor), cancellationToken))
            : Forbid();

    /// <summary>«Вартість складу»: positive balances at today's prices of «Тканини»; shortages apart.</summary>
    [HttpGet("valuation", Name = "getFabricValuation")]
    [Authorize(Policy = WarehouseAccess.FabricValuation)]
    public async Task<ActionResult<FabricValuationResponse>> Valuation(CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetFabricValuationQuery(actor), cancellationToken))
            : Forbid();

    /// <summary>«Фурнітура до замовлення»: the full accessory need of open orders by model composition.</summary>
    [HttpGet("hardware", Name = "getHardwareOrder")]
    [Authorize(Policy = WarehouseAccess.HardwareOrder)]
    public async Task<ActionResult<HardwareOrderResponse>> Hardware(CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetHardwareOrderQuery(actor), cancellationToken))
            : Forbid();
}
