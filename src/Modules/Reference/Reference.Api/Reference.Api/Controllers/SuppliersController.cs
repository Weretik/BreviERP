using Reference.Api.Contracts.Suppliers;
using Reference.Application.Features.Supplier.Create;
using Reference.Application.Features.Supplier.Create.DTOs;
using Reference.Application.Features.Supplier.Delete;
using Reference.Application.Features.Supplier.GetList;
using Reference.Application.Features.Supplier.GetList.DTOs;
using Reference.Application.Features.Supplier.Update;
using Reference.Application.Features.Supplier.Update.DTOs;

namespace Reference.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/reference/suppliers")]
public sealed class SuppliersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<SupplierRowDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<SupplierRowDTO>>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSuppliersQuery(), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSupplierCommand(
            new CreateSupplierCommandRequest(
                request.Id,
                request.Name,
                request.Link,
                request.ContactPerson,
                request.PhoneNumber,
                request.Notes));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSupplierCommand(
            id,
            new UpdateSupplierCommandRequest(
                request.Name,
                request.Link,
                request.ContactPerson,
                request.PhoneNumber,
                request.Notes));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteSupplierCommand(id), cancellationToken);

        return this.ToActionResult(result);
    }
}
