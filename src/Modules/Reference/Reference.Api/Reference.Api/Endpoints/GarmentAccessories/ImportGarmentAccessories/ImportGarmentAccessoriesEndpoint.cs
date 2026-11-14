using Reference.Application.Features.GarmentAccessory.Commands.ImportGarmentAccessories;
using Reference.Application.Features.SheetImport.Shared;

namespace Reference.Api.Endpoints.GarmentAccessories.ImportGarmentAccessories;

/// <summary>Imports the Calculator sheet «Фурнітура» read by the admin: dryRun returns the preview; otherwise the rows are
/// saved when none has an error.</summary>
internal static class ImportGarmentAccessoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync).Produces<ReferenceImportResult>()
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(ImportGarmentAccessoriesRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ImportGarmentAccessoriesCommand(request.Rows, request.DryRun,
            request.DeactivateMissing), cancellationToken)).ToHttpResult();
}

/// <summary>Rows of the Calculator sheet «Фурнітура»; dryRun returns the preview without saving.</summary>
public sealed record ImportGarmentAccessoriesRequest(
    IReadOnlyList<GarmentAccessoryImportRow> Rows,
    bool DryRun = true,
    bool DeactivateMissing = false);
