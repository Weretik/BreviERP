using Microsoft.AspNetCore.RateLimiting;
using Reference.Application.Features.Fabric.Commands.ImportFabrics;
using Reference.Application.Features.SheetImport.Shared;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.Fabrics.ImportFabrics;

/// <summary>Imports the Calculator sheet «Тканини» read by the admin: dryRun returns the preview; otherwise the rows are
/// saved when none has an error.</summary>
internal static class ImportFabricsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync)
            .WithName("importFabrics").RequireRateLimiting("HeavyOperation")
            .WithSummary("Import the Calculator sheet «Тканини»")
            .WithDescription("Requires access:materials. The caller sends the sheet rows as JSON (one-time transfer from the Calculator sheet). A row is matched by code (FAB-…); an older fabric without a code is linked when its name matches (case and punctuation ignored); the rest are created with the next ID. Supplier names are matched like the KeepinCRM sync; an unknown one becomes a new manual supplier, an empty one «Не вказано». dryRun (default) returns the preview only. Otherwise nothing is saved when any row has an error. deactivateMissing marks fabrics that are not in the sheet as not used. Repeating the same file changes nothing.")
            .Produces<ReferenceImportResult>()
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(ImportFabricsRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new ImportFabricsCommand(request.Rows, request.DryRun, request.DeactivateMissing),
            cancellationToken)).ToHttpResult();
}

/// <summary>Rows of the Calculator sheet «Тканини»; dryRun returns the preview without saving.</summary>
public sealed record ImportFabricsRequest(
    IReadOnlyList<FabricImportRow> Rows,
    bool DryRun = true,
    bool DeactivateMissing = false);
