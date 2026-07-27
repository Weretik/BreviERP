using Catalog.Api.Contracts.ProductCategories;
using Catalog.Application.Features.ProductCategory.Create;
using Catalog.Application.Features.ProductCategory.Create.DTOs;
using Catalog.Application.Features.ProductCategory.Delete;
using Catalog.Application.Features.ProductCategory.GetAdminList;
using Catalog.Application.Features.ProductCategory.GetAdminList.DTOs;
using Catalog.Application.Features.ProductCategory.GetStoreList;
using Catalog.Application.Features.ProductCategory.GetStoreList.DTOs;
using Catalog.Application.Features.ProductCategory.Update;
using Catalog.Application.Features.ProductCategory.Update.DTOs;

namespace Catalog.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/reference/product-categories")]
public sealed class ProductCategoriesController(ISender sender) : ControllerBase
{
    [HttpGet("admin")]
    [ProducesResponseType(typeof(List<AdminProductCategoryRowDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AdminProductCategoryRowDTO>>> GetAdmin(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminProductCategoriesQuery(), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("store/{language}")]
    [ProducesResponseType(typeof(List<StoreProductCategoryRowDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<StoreProductCategoryRowDTO>>> GetStore(
        [FromRoute] string language,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreProductCategoriesQuery(language), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreateProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCategoryCommand(
            new CreateProductCategoryCommandRequest(
                request.Id,
                request.Name,
                request.RuName,
                request.Slug,
                request.ParentId,
                request.SortOrder,
                request.IsActive,
                request.Description));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateProductCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCategoryCommand(
            id,
            new UpdateProductCategoryCommandRequest(
                request.Name,
                request.RuName,
                request.Slug,
                request.ParentId,
                request.SortOrder,
                request.IsActive,
                request.Description));

        var result = await sender.Send(command, cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteProductCategoryCommand(id), cancellationToken);

        return this.ToActionResult(result);
    }
}


