using Reference.Application.Features.ProductSection.Commands.ImportProductSections;
using BuildingBlocks.Api.Errors;

namespace Reference.Api.Endpoints.ProductSections.ImportProductSections;

/// <summary>Imports the sheet «Типи і розділи»: dryRun returns the preview; otherwise the rows are saved when none has
/// an error.</summary>
internal static class ImportProductSectionsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("import", HandleAsync)
            .WithName("importProductSections")
            .WithSummary("Import the Calculator sheet «Типи і розділи»")
            .WithDescription("Requires access:product-sections. Sections are matched by name (case and punctuation ignored); the row number becomes the display order. dryRun (default) returns the preview only. Otherwise nothing is saved when any row has an error. deleteMissing deletes sections that are not in the sheet. Repeating the same file changes nothing.")
            .Produces<ProductSectionImportResult>()
            .RequireAuthorization("access:product-sections");

    private static async Task<IResult> HandleAsync(ImportProductSectionsRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ImportProductSectionsCommand(request.Rows, request.DryRun, request.DeleteMissing),
            cancellationToken)).ToHttpResult();
}

/// <summary>Rows of the Calculator sheet «Типи і розділи»; dryRun returns the preview without saving.</summary>
public sealed record ImportProductSectionsRequest(
    IReadOnlyList<ProductSectionImportRow> Rows,
    bool DryRun = true,
    bool DeleteMissing = false);
