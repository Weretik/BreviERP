using System.Security.Claims;
using Catalog.Api.Contracts.Products.Photos;
using Catalog.Application.Features.Product.Commands.ImportOldSitePhotos;

namespace Catalog.Api.Controllers;

/// <summary>Photos and missing sewing models from the old site (CAT-016); Admin only, dry run first.</summary>
[ApiController]
[Route("api/v1/products/old-site-photo-import")]
public sealed class OldSitePhotoImportController(ISender sender) : ControllerBase
{
    /// <summary>
    /// dryRun=true reports what would be found, created and downloaded. A real run takes at most 20 products; the
    /// admin page sends the file in parts. A repeat adds nothing twice.
    /// </summary>
    [HttpPost(Name = "importOldSitePhotos")]
    [Authorize(Policy = "RequireAdminRole")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [ProducesResponseType(typeof(OldSitePhotoImportReport), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OldSitePhotoImportReport>> Import(
        [FromBody] ImportOldSitePhotosRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Forbid();
        return this.ToActionResult(await sender.Send(new ImportOldSitePhotosCommand(
            request.DryRun, request.BaseUrl, request.Products ?? [], actor), cancellationToken));
    }
}
