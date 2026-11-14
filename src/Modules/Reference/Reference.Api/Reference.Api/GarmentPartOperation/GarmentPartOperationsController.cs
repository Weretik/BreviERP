using Reference.Application.Features.GarmentPartOperation.Commands.CreateGarmentPartOperation;
using Reference.Application.Features.GarmentPartOperation.Commands.DeleteGarmentPartOperation;
using Reference.Application.Features.GarmentPartOperation.Commands.ImportGarmentPartOperations;
using Reference.Application.Features.GarmentPartOperation.Commands.UpdateGarmentPartOperation;
using Reference.Application.Features.GarmentPartOperation.Queries.GetGarmentPartOperations;

namespace Reference.Api.GarmentPartOperation;

[ApiController]
[Route("api/reference/garment-part-operations")]
public sealed class GarmentPartOperationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(List<GarmentPartOperationRowDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GarmentPartOperationRowDTO>>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGarmentPartOperationsQuery(), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = "access:operations")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreateGarmentPartOperationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateGarmentPartOperationCommand(
            new CreateGarmentPartOperationCommandRequest(
                request.Id,
                request.GarmentPartName,
                request.Name,
                request.Min,
                request.Code,
                request.Element,
                request.Variant,
                request.MinutesPerProduct,
                request.IsActive,
                request.Notes));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    /// <summary>
    /// Imports the Calculator sheet «Операції»: dryRun returns the preview; otherwise the rows are saved when none
    /// has an error. With deleteMissing, operations and groups that are not in the sheet are deleted; an operation
    /// still used by products or price history is marked «Не використовується» instead.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Policy = "access:operations")]
    [ProducesResponseType(typeof(GarmentPartOperationImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GarmentPartOperationImportResult>> Import(
        [FromBody] ImportGarmentPartOperationsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ImportGarmentPartOperationsCommand(request.Rows, request.DryRun, request.DeleteMissing),
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "access:operations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateGarmentPartOperationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateGarmentPartOperationCommand(
            id,
            new UpdateGarmentPartOperationCommandRequest(
                request.GarmentPartName,
                request.Name,
                request.Min,
                request.Code,
                request.Element,
                request.Variant,
                request.MinutesPerProduct,
                request.IsActive,
                request.Notes));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "access:operations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteGarmentPartOperationCommand(id), cancellationToken);

        return this.ToActionResult(result);
    }
}
