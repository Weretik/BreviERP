using Reference.Api.Contracts.GarmentAccessories;
using Reference.Application.Features.GarmentAccessory.Create;
using Reference.Application.Features.GarmentAccessory.Create.DTOs;
using Reference.Application.Features.GarmentAccessory.Delete;
using Reference.Application.Features.GarmentAccessory.GetList;
using Reference.Application.Features.GarmentAccessory.GetList.DTOs;
using Reference.Application.Features.GarmentAccessory.Import;
using Reference.Application.Features.Shared.SheetImport;
using Reference.Application.Features.GarmentAccessory.Update;
using Reference.Application.Features.GarmentAccessory.Update.DTOs;

namespace Reference.Api.Controllers;

[ApiController]
[Route("api/reference/garment-accessories")]
public sealed class GarmentAccessoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GarmentAccessoryRowDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GarmentAccessoryRowDTO>>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGarmentAccessoriesQuery(), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageProducts")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreateGarmentAccessoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateGarmentAccessoryCommand(
            new CreateGarmentAccessoryCommandRequest(
                request.Id,
                request.Name,
                request.Price,
                request.SupplierName,
                request.Code,
                request.Unit,
                request.IsActive,
                request.Comment));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    /// <summary>
    /// Imports the Calculator sheet «Фурнітура» read by the admin: dryRun returns the preview; otherwise the rows are
    /// saved when none has an error.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Policy = "CanManageProducts")]
    [ProducesResponseType(typeof(ReferenceImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReferenceImportResult>> Import(
        [FromBody] ImportGarmentAccessoriesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ImportGarmentAccessoriesCommand(request.Rows, request.DryRun, request.DeactivateMissing), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageProducts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateGarmentAccessoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateGarmentAccessoryCommand(
            id,
            new UpdateGarmentAccessoryCommandRequest(
                request.Name,
                request.Price,
                request.SupplierName,
                request.Code,
                request.Unit,
                request.IsActive,
                request.Comment));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageProducts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteGarmentAccessoryCommand(id), cancellationToken);

        return this.ToActionResult(result);
    }
}
