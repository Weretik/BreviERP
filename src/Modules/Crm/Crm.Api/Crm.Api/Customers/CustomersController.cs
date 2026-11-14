using Ardalis.Result;
using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Crm.Application.Features.Customers.Commands.CreateCustomer;
using Crm.Application.Features.Customers.Commands.MergeCustomers;
using Crm.Application.Features.Customers.Commands.ReviseCustomer;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;
using Crm.Application.Features.Customers.Queries.GetCustomer;
using Crm.Application.Features.Customers.Queries.GetCustomers;
namespace Crm.Api.Customers;

[ApiController]
[Authorize(Policy = CrmAccess.Counterparties)]
[Route("api/crm/customers")]
public sealed class CustomersController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCrmCustomers")]
    public async Task<ActionResult> List([FromQuery] string? search, [FromQuery] bool? active,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (page is < 1 or > 1_000_000 || pageSize is < 1 or > 100 || search?.Length > 300) return BadRequest("invalid_pagination");
        var result = await sender.Send(new GetCustomersQuery(actor, search, active, page, pageSize), cancellationToken);
        return Ok(new { value = result.Value, pagedInfo = result.PagedInfo });
    }

    [HttpGet("{customerId:guid}", Name = "getCrmCustomer")]
    public async Task<ActionResult> Detail(Guid customerId, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new GetCustomerQuery(customerId, actor), cancellationToken));
    }

    [HttpPost(Name = "createCrmCustomer")]
    public async Task<ActionResult> Create(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new CreateCustomerCommand(request.Input, request.RequestId, actor), cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(Detail), new { customerId = result.Value.PublicId }, result.Value) : this.ToActionResult(result);
    }

    [HttpPut("{customerId:guid}", Name = "reviseCrmCustomer")]
    public async Task<ActionResult> Revise(Guid customerId, ReviseCustomerRequest request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new ReviseCustomerCommand(customerId,
            request.ExpectedRevision, request.Input, request.Active, request.RequestId, actor), cancellationToken));
    }

    [HttpPost("{customerId:guid}/merge", Name = "mergeCrmCustomers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Merge(Guid customerId, MergeCustomersRequest request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new MergeCustomersCommand(customerId,
            request.VictimId, request.ExpectedRevision, request.VictimRevision, request.Reason, request.RequestId, actor), cancellationToken));
    }

    /// <summary>«Оновити з KeepinCRM»: the same run as the daily job; removeUnlinked deletes, so it is admin-only.</summary>
    [HttpPost("keepin-sync", Name = "syncCrmCustomersFromKeepinCrm")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult> SyncKeepinCrm(SyncKeepinCrmClientsRequest? request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        request ??= new SyncKeepinCrmClientsRequest();
        if (request.RemoveUnlinked && !actor.IsAdmin) return Forbid();
        var result = await sender.Send(new SyncKeepinCrmClientsCommand(request.DryRun, request.RemoveUnlinked), cancellationToken);
        return result.Status == ResultStatus.Unavailable
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, new { errors = result.Errors })
            : this.ToActionResult(result);
    }
}
