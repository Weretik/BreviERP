using BuildingBlocks.Api.Result;
using Crm.Api.Customers.Requests;
using Crm.Api.Access;
using Crm.Application.Customers.SaveCustomerContact;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Crm.Api.Customers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/customers/{customerId:guid}/contacts")]
public sealed class CustomerContactsController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "createCrmCustomerContact")]
    public Task<ActionResult> Create(Guid customerId, SaveCustomerContactRequest request, CancellationToken cancellationToken)
        => Save(customerId, null, request, cancellationToken);

    [HttpPut("{contactId:guid}", Name = "reviseCrmCustomerContact")]
    public Task<ActionResult> Revise(Guid customerId, Guid contactId, SaveCustomerContactRequest request, CancellationToken cancellationToken)
        => Save(customerId, contactId, request, cancellationToken);

    private async Task<ActionResult> Save(Guid customerId, Guid? contactId, SaveCustomerContactRequest request, CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(new SaveCustomerContactCommand(customerId,
            contactId, request.ExpectedRevision, request.Input, request.RequestId, actor), cancellationToken));
    }
}
