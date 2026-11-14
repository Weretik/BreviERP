using Catalog.Application.Features.Product.Commands.ImportOldSitePhotos;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Photos.ImportOldSitePhotos;

/// <summary>
/// dryRun=true reports what would be found, created and downloaded. A real run takes at most 20 products; the
/// admin page sends the file in parts. A repeat adds nothing twice.
/// </summary>
internal static class ImportOldSitePhotosEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("importOldSitePhotos")
            .RequireAuthorization("RequireAdminRole")
            .WithMetadata(new RequestSizeLimitAttribute(10 * 1024 * 1024));

    private static async Task<IResult> HandleAsync(ImportOldSitePhotosRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        return (await sender.Send(new ImportOldSitePhotosCommand(
            request.DryRun, request.BaseUrl, request.Products ?? [], actor), cancellationToken)).ToHttpResult();
    }
}
