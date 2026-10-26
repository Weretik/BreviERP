using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Crm.Api.Customers.Requests;
using Crm.Application.Customers.CreateCustomer;
using Crm.Application.Customers.GetCustomer;
using Crm.Application.Customers.GetCustomers;
using Crm.Application.Customers.MergeCustomers;
using Crm.Application.Customers.Models;
using Crm.Application.Customers.ReviseCustomer;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Crm.Api.Customers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
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
}
