using Microsoft.AspNetCore.RateLimiting;
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
        group.MapPost("", HandleAsync).WithName("importOldSitePhotos").RequireRateLimiting("HeavyOperation")
            .WithSummary("Photos and missing sewing models from the old site")
            .WithDescription("Admin only. A product is found by an old-site SKU (article SKUs of a sewing model, the site article of PPE) or by newModel name; a missing sewing model with newModel is created switched off. Photos are downloaded from baseUrl + path into the Media folder «Старий сайт» (old subfolders kept) and added after the product's own photos; the old main photo becomes main only for a product without photos. dryRun reports and changes nothing; a real run takes at most 20 products, so the admin page sends the file in parts. A repeat adds nothing twice.")
            .RequireAuthorization("RequireAdminRole")
            .WithMetadata(new RequestSizeLimitAttribute(10 * 1024 * 1024))
            .Produces<OldSitePhotoImportReport>();

    private static async Task<IResult> HandleAsync(ImportOldSitePhotosRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        return (await sender.Send(new ImportOldSitePhotosCommand(
            request.DryRun, request.BaseUrl, request.Products ?? [], actor), cancellationToken)).ToHttpResult();
    }
}
