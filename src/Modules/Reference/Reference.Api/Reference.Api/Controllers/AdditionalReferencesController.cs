using Reference.Application.Features.AdditionalReference.Create;
using Reference.Application.Features.AdditionalReference.Create.DTOs;
using Reference.Application.Features.AdditionalReference.Delete;
using Reference.Application.Features.AdditionalReference.GetList;
using Reference.Application.Features.AdditionalReference.GetList.DTOs;
using Reference.Application.Features.AdditionalReference.Update;
using Reference.Application.Features.AdditionalReference.Update.DTOs;
using Reference.Api.Contracts.AdditionalReferences;

namespace Reference.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/reference/additional-references")]
public sealed class AdditionalReferencesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AdditionalReferenceRowDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AdditionalReferenceRowDTO>>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdditionalReferenceQuery(), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreateAdditionalReferenceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateAdditionalReferenceCommand(
            new CreateAdditionalReferenceCommandRequest(
                request.Id,
                request.Name,
                request.Key,
                request.Value,
                request.Unit,
                request.Description));

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
        [FromBody] UpdateAdditionalReferenceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAdditionalReferenceCommand(
            id,
            new UpdateAdditionalReferenceCommandRequest(
                request.Name,
                request.Key,
                request.Value,
                request.Unit,
                request.Description));

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
        var result = await sender.Send(new DeleteAdditionalReferenceCommand(id), cancellationToken);

        return this.ToActionResult(result);
    }
}
