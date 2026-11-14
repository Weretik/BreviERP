using Reference.Application.Features.GarmentPartOperation.Commands.ImportGarmentPartOperations;

namespace Reference.Api.Endpoints.GarmentPartOperations;

/// <summary>Imports the Calculator sheet «Операції»: dryRun returns the preview; otherwise the rows are saved when none
/// has an error. With deleteMissing, operations and groups that are not in the sheet are deleted; an operation
/// still used by products or price history is marked «Не використовується» instead.</summary>
internal static class ImportGarmentPartOperationsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync).Produces<GarmentPartOperationImportResult>()
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
