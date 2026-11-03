using Reference.Api.Contracts.Suppliers;
using Reference.Application.Features.Supplier.Create;
using Reference.Application.Features.Supplier.Create.DTOs;
using Reference.Application.Features.Supplier.Delete;
using Reference.Application.Features.Supplier.GetList;
using Reference.Application.Features.Supplier.GetList.DTOs;
using Reference.Application.Features.Supplier.KeepinCrmSync;
using Reference.Application.Features.Supplier.KeepinCrmSync.DTOs;
using Reference.Application.Features.Supplier.UpdateBreviFields;
using Reference.Application.Features.Supplier.Update;
using Reference.Application.Features.Supplier.Update.DTOs;

namespace Reference.Api.Controllers;

[ApiController]
[Authorize(Policy = "CanManageProducts")]
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
                request.Notes,
                request.Email,
                request.PurchaseCategories,
                request.IsActive));

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
                request.Notes,
                request.Email,
                request.PurchaseCategories,
                request.IsActive));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    /// <summary>Activity and notes: always editable, also for a KeepinCRM supplier.</summary>
    [HttpPut("{id:int}/brevi-fields")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateBreviFields(
        [FromRoute] int id,
        [FromBody] UpdateSupplierBreviFieldsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateSupplierBreviFieldsCommand(id, request.IsActive, request.Notes), cancellationToken);

        return this.ToActionResult(result);
    }

    /// <summary>Reads all suppliers from KeepinCRM and updates Brevi; the same run as the daily job.</summary>
    [HttpPost("keepin-sync")]
    [ProducesResponseType(typeof(KeepinCrmSupplierSyncResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<KeepinCrmSupplierSyncResult>> SyncKeepinCrm(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SyncKeepinCrmSuppliersCommand(), cancellationToken);
        if (result.Status == Ardalis.Result.ResultStatus.Unavailable)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { errors = result.Errors });

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
