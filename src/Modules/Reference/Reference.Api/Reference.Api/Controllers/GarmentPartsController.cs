using Reference.Api.Contracts.GarmentParts;
using Reference.Application.Features.GarmentPart.Create;
using Reference.Application.Features.GarmentPart.Create.DTOs;
using Reference.Application.Features.GarmentPart.Delete;
using Reference.Application.Features.GarmentPart.GetList;
using Reference.Application.Features.GarmentPart.GetList.DTOs;
using Reference.Application.Features.GarmentPart.Update;
using Reference.Application.Features.GarmentPart.Update.DTOs;

namespace Reference.Api.Controllers;

[ApiController]
[Route("api/reference/garment-parts")]
public sealed class GarmentPartsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<GarmentPartRowDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GarmentPartRowDTO>>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGarmentPartsQuery(), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreateGarmentPartRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateGarmentPartCommand(
            new CreateGarmentPartCommandRequest(request.Id, request.Name));

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
        [FromBody] UpdateGarmentPartRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateGarmentPartCommand(
            id,
            new UpdateGarmentPartCommandRequest(request.Name));

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
        var result = await sender.Send(new DeleteGarmentPartCommand(id), cancellationToken);

        return this.ToActionResult(result);
    }
}
