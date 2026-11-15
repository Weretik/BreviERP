using Reference.Application.Features.GarmentPartOperation.Commands.ImportGarmentPartOperations;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.GarmentPartOperations.ImportGarmentPartOperations;

/// <summary>Imports the Calculator sheet «Операції»: dryRun returns the preview; otherwise the rows are saved when none
/// has an error. With deleteMissing, operations and groups that are not in the sheet are deleted; an operation
/// still used by products or price history is marked «Не використовується» instead.</summary>
internal static class ImportGarmentPartOperationsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync)
            .WithName("importGarmentPartOperations")
            .WithSummary("Import the Calculator sheet «Операції»")
            .WithDescription("Requires access:operations. The caller sends the sheet rows as JSON (one-time transfer from the Calculator sheet). A row is matched by code (OP-…); the rest are created with the next ID. «Група деталей» is matched by name (case and punctuation ignored); an unknown one becomes a new group. dryRun (default) returns the preview only. Otherwise nothing is saved when any row has an error. deleteMissing deletes operations that are not in the sheet; an operation still used by a product or price history is marked not used instead (keptInUse). Groups left without operations are then deleted. Repeating the same file changes nothing.")
            .Produces<GarmentPartOperationImportResult>()
            .RequireAuthorization("access:operations");

    private static async Task<IResult> HandleAsync(ImportGarmentPartOperationsRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ImportGarmentPartOperationsCommand(request.Rows, request.DryRun,
            request.DeleteMissing), cancellationToken)).ToHttpResult();
}

/// <summary>Rows of the Calculator sheet «Операції»; dryRun returns the preview without saving.</summary>
public sealed record ImportGarmentPartOperationsRequest(
    IReadOnlyList<GarmentPartOperationImportRow> Rows,
    bool DryRun = true,
    bool DeleteMissing = false);
