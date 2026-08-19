using Catalog.Api.Contracts.Products;
using Catalog.Application.Features.Product.Create;
using Catalog.Application.Features.Product.Delete;
using Catalog.Application.Features.Product.GetAdminDetail;
using Catalog.Application.Features.Product.GetAdminList;
using Catalog.Application.Features.Product.Update;

namespace Catalog.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ProductListPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductListPageResponse>> GetList(
        [FromQuery] GetAdminProductsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        return Ok(ProductListResponseMapper.ToResponse(result));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailResponse>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminProductDetailQuery(id), cancellationToken);

        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Ok(ProductDetailResponseMapper.ToResponse(result.Value))
            : this.ToActionResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDetailResponse>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (!ProductWriteRequestMapper.TryMap(request, out var commandRequest))
            return BadRequest("type must be either Sewing or Ppe.");

        var result = await sender.Send(new CreateProductCommand(commandRequest!), cancellationToken);

        if (result.Status != Ardalis.Result.ResultStatus.Ok)
            return this.ToActionResult(result);

        var response = ProductDetailResponseMapper.ToResponse(result.Value);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDetailResponse>> Replace(
        [FromRoute] int id,
        [FromBody] ProductWriteRequest request,
        CancellationToken cancellationToken)
    {
        if (!ProductWriteRequestMapper.TryMap(request, out var commandRequest))
            return BadRequest("type must be either Sewing or Ppe.");

        var result = await sender.Send(new ReplaceProductCommand(id, commandRequest!), cancellationToken);

        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Ok(ProductDetailResponseMapper.ToResponse(result.Value))
            : this.ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteProductCommand(id), cancellationToken);

        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? NoContent()
            : this.ToActionResult(result);
    }
}
