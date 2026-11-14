using Reference.Application.Features.FabricColor.Shared;
using Reference.Application.Features.FabricColor.Commands.CreateFabricColor;
using Reference.Application.Features.FabricColor.Commands.DeleteFabricColor;
using Reference.Application.Features.FabricColor.Commands.UpdateFabricColor;
using Reference.Application.Features.FabricColor.Queries.GetFabricColors;

namespace Reference.Api.FabricColor;

/// <summary>«Кольори» of «Матеріали»: the sheet column «Загальний перелік кольорів».</summary>
[ApiController]
[Route("api/reference/fabric-colors")]
public sealed class FabricColorsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getFabricColors")]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(List<FabricColorRowDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FabricColorRowDTO>>> Get(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetFabricColorsQuery(), cancellationToken));

    [HttpPost(Name = "createFabricColor")]
    [Authorize(Policy = "access:materials")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> Create([FromBody] FabricColorRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new CreateFabricColorCommand(Fields(request)), cancellationToken));

    [HttpPut("{id:int}", Name = "updateFabricColor")]
    [Authorize(Policy = "access:materials")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] FabricColorRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateFabricColorCommand(id, Fields(request)), cancellationToken));

    [HttpDelete("{id:int}", Name = "deleteFabricColor")]
    [Authorize(Policy = "access:materials")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteFabricColorCommand(id), cancellationToken));

    private static FabricColorFields Fields(FabricColorRequest request) =>
        new(request.Name, request.IsActive, request.SortOrder);
}
