using Reference.Api.Contracts.GarmentAccessories;
using Reference.Application.Features.GarmentAccessory.Create;
using Reference.Application.Features.GarmentAccessory.Create.DTOs;
using Reference.Application.Features.GarmentAccessory.Delete;
using Reference.Application.Features.GarmentAccessory.GetList;
using Reference.Application.Features.GarmentAccessory.GetList.DTOs;
using Reference.Application.Features.GarmentAccessory.Update;
using Reference.Application.Features.GarmentAccessory.Update.DTOs;

namespace Reference.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/reference/garment-accessories")]
public sealed class GarmentAccessoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<GarmentAccessoryRowDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<GarmentAccessoryRowDTO>>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGarmentAccessoriesQuery(), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreateGarmentAccessoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateGarmentAccessoryCommand(
            new CreateGarmentAccessoryCommandRequest(request.Id, request.Name, request.Price));

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
        [FromBody] UpdateGarmentAccessoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateGarmentAccessoryCommand(
            id,
            new UpdateGarmentAccessoryCommandRequest(request.Name, request.Price));

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
        var result = await sender.Send(new DeleteGarmentAccessoryCommand(id), cancellationToken);

        return this.ToActionResult(result);
    }
}
