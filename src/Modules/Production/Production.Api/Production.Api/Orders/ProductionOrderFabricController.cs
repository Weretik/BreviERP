using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.Stock.Issues;

namespace Production.Api.Orders;

/// <summary>Tab «Тканина» of an item: plan by colour, «Видати» and «Сторно видачі».</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.Orders)]
[Route("api/production/orders")]
public sealed class ProductionOrderFabricController(ISender sender) : ControllerBase
{
    [HttpGet("{id:int}/items/{itemId:int}/fabric", Name = "getProductionItemFabric")]
    public async Task<ActionResult<ProductionItemFabricResponse>> Get(int id, int itemId,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetProductionItemFabricQuery(actor, id, itemId), cancellationToken))
            : Forbid();

    [HttpPost("{id:int}/items/{itemId:int}/fabric-issues", Name = "createFabricIssue")]
    public async Task<ActionResult<ProductionItemFabricResponse>> Issue(int id, int itemId,
        CreateFabricIssueInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CreateFabricIssueCommand(actor, id, itemId, input), cancellationToken))
            : Forbid();

    [HttpPost("{id:int}/fabric-issues/{issueId:int}/reverse", Name = "reverseFabricIssue")]
    public async Task<ActionResult<ProductionItemFabricResponse>> Reverse(int id, int issueId,
        ReverseFabricIssueInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ReverseFabricIssueCommand(actor, id, issueId, input), cancellationToken))
            : Forbid();
}
