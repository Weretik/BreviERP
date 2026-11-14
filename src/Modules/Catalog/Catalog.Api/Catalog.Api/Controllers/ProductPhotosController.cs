using Catalog.Api.Contracts.Products.Photos;
using Catalog.Application.Features.Product.Commands.SetProductPhotos;
using Catalog.Application.Features.Product.Queries.GetProductPhotos;
using Catalog.Application.Features.Product.Shared.Photos;

namespace Catalog.Api.Controllers;

/// <summary>
/// The tab «Фото» of a product card (CAT-016). Every staff member who reads the catalogue sees it; only Admin (the
/// Director also holds Admin) adds, removes, reorders photos and picks the main one.
/// </summary>
[ApiController]
[Route("api/v1/products/{id:int}/photos")]
public sealed class ProductPhotosController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getProductPhotos")]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(ProductPhotoList), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPhotoList>> Get([FromRoute] int id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductPhotosQuery(id), cancellationToken));

    [HttpPut(Name = "setProductPhotos")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(ProductPhotoList), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductPhotoList>> Set(
        [FromRoute] int id, [FromBody] SetProductPhotosRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(
            new SetProductPhotosCommand(id, request.MediaFileIds ?? [], request.MainMediaFileId), cancellationToken));
}
