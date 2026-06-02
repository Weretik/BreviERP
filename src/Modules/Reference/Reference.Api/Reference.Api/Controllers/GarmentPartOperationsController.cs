using Reference.Api.Contracts.GarmentPartOperations;
using Reference.Application.Features.GarmentPartOperation.Create;
using Reference.Application.Features.GarmentPartOperation.Create.DTOs;
using Reference.Application.Features.GarmentPartOperation.Delete;
using Reference.Application.Features.GarmentPartOperation.GetList;
using Reference.Application.Features.GarmentPartOperation.GetList.DTOs;
using Reference.Application.Features.GarmentPartOperation.Update;
using Reference.Application.Features.GarmentPartOperation.Update.DTOs;

namespace Reference.Api.Controllers;

[ApiController]
[Route("api/reference/garment-part-operations")]
public sealed class GarmentPartOperationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<GarmentPartOperationRowDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GarmentPartOperationRowDTO>>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGarmentPartOperationsQuery(), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreateGarmentPartOperationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateGarmentPartOperationCommand(
            new CreateGarmentPartOperationCommandRequest(
                request.Id,
                request.GarmentPartName,
                request.Name,
                request.Min));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
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
                request.Min));

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
        var result = await sender.Send(new DeleteGarmentPartOperationCommand(id), cancellationToken);

        return this.ToActionResult(result);
    }
}

