using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Archive.GetArchive;
using Production.Application.Archive.ImportArchive;
using Production.Application.Contracts;

namespace Production.Api.Archive;

/// <summary>«Архів цеху» (PRD-D54): read-only lines of the old «Цех» for everyone with production orders.</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.Archive)]
[Route("api/production/archive")]
public sealed class WorkshopArchiveController(ISender sender) : ControllerBase
{
    /// <param name="kind">Schedule («ГРАФИК работы») or Request (the old requests).</param>
    [HttpGet(Name = "getWorkshopArchive")]
    public async Task<ActionResult<WorkshopArchivePageResponse>> List([FromQuery] string kind,
        [FromQuery] string? search, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(
                new GetWorkshopArchiveQuery(actor, kind, search, from, to, page, pageSize), cancellationToken))
            : Forbid();

    /// <summary>Moves workshop-archive.json (Admin); <c>dryRun</c> only counts; a line already moved is skipped.</summary>
    [HttpPost("import", Name = "importWorkshopArchive")]
    public async Task<ActionResult<WorkshopArchiveImportResult>> Import(
        [FromBody] IReadOnlyList<WorkshopArchiveImportRow> rows, [FromQuery] bool dryRun,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ImportWorkshopArchiveCommand(actor, rows, dryRun),
                cancellationToken))
            : Forbid();
}
