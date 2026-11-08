using Reference.Api.Contracts.Fabrics;
using Reference.Application.Features.Fabric.Create;
using Reference.Application.Features.Fabric.Create.DTOs;
using Reference.Application.Features.Fabric.Delete;
using Reference.Application.Features.Fabric.GetList;
using Reference.Application.Features.Fabric.GetList.DTOs;
using Reference.Application.Features.Fabric.Import;
using Reference.Application.Features.Shared.SheetImport;
using Reference.Application.Features.Fabric.Update;
using Reference.Application.Features.Fabric.Update.DTOs;

namespace Reference.Api.Controllers;

[ApiController]
[Route("api/reference/fabrics")]

public sealed class FabricsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(List<FabricRowDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<FabricRowDTO>>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllFabricQuery(), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = "access:materials")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreateFabricRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateFabricCommand(
            new CreateFabricCommandRequest(
                request.Id, request.Name, request.Price, request.ProviderName, request.Code, request.NameRu,
                request.Composition, request.CompositionRu, request.Density, request.IsActive, request.Comment,
                request.ProposalDescription));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    /// <summary>
    /// Imports the Calculator sheet «Тканини» read by the admin: dryRun returns the preview; otherwise the rows are
    /// saved when none has an error.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Policy = "access:materials")]
    [ProducesResponseType(typeof(ReferenceImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReferenceImportResult>> Import(
        [FromBody] ImportFabricsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ImportFabricsCommand(request.Rows, request.DryRun, request.DeactivateMissing), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "access:materials")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateFabricRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateFabricCommand(
            id,
            new UpdateFabricCommandRequest(
                request.Name, request.Price, request.ProviderName, request.Code, request.NameRu,
                request.Composition, request.CompositionRu, request.Density, request.IsActive, request.Comment,
                request.ProposalDescription));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "access:materials")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteFabricCommand(id), cancellationToken);

        return this.ToActionResult(result);
    }
}
