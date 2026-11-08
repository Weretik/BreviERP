using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.FabricNorms.DeleteNorm;
using Production.Application.FabricNorms.GetNorms;
using Production.Application.FabricNorms.ImportNorms;
using Production.Application.FabricNorms.SaveNorm;

namespace Production.Api.FabricNorms;

/// <summary>«Норми витрати тканини» per Catalog sewing model, in metres.</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.FabricNorms)]
[Route("api/production/fabric-norms")]
public sealed class FabricNormsController(ISender sender) : ControllerBase
{
    /// <summary>Every Catalog sewing model with its norm (or none), then norms whose model is gone.</summary>
    [HttpGet(Name = "getFabricNorms")]
    public async Task<ActionResult<IReadOnlyList<FabricNormRowResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetFabricNormsQuery(), cancellationToken));

    [HttpPut("{modelCode}", Name = "saveFabricNorm")]
    public async Task<ActionResult<FabricNormRowResponse>> Save(string modelCode, FabricNormInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SaveFabricNormCommand(modelCode, input, this.UserId()), cancellationToken));

    [HttpDelete("{modelCode}", Name = "deleteFabricNorm")]
    public async Task<ActionResult> Delete(string modelCode, CancellationToken cancellationToken) =>
        this.ToNoContentResult(await sender.Send(new DeleteFabricNormCommand(modelCode), cancellationToken));

    /// <summary>Loads norms from a list; with <c>dryRun</c> only shows what would change. If a row is wrong nothing is saved and its line says why.</summary>
    [HttpPost("import", Name = "importFabricNorms")]
    public async Task<ActionResult<FabricNormImportResult>> Import(
        [FromBody] IReadOnlyList<FabricNormImportRow> rows, [FromQuery] bool dryRun, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new ImportFabricNormsCommand(rows, dryRun, this.UserId()), cancellationToken));
}
