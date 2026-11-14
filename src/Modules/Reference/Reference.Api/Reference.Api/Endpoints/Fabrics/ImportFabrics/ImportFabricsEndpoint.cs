using Reference.Application.Features.Fabric.Commands.ImportFabrics;
using Reference.Application.Features.SheetImport.Shared;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.Fabrics.ImportFabrics;

/// <summary>Imports the Calculator sheet «Тканини» read by the admin: dryRun returns the preview; otherwise the rows are
/// saved when none has an error.</summary>
internal static class ImportFabricsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync).Produces<ReferenceImportResult>()
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
