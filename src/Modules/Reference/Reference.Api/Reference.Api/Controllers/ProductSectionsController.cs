using Reference.Api.Contracts.ProductSections;
using Reference.Application.Features.ProductSection.Create;
using Reference.Application.Features.ProductSection.Create.DTOs;
using Reference.Application.Features.ProductSection.Delete;
using Reference.Application.Features.ProductSection.GetList;
using Reference.Application.Features.ProductSection.GetList.DTOs;
using Reference.Application.Features.ProductSection.Import;
using Reference.Application.Features.ProductSection.Import.DTOs;
using Reference.Application.Features.ProductSection.Update;
using Reference.Application.Features.ProductSection.Update.DTOs;

namespace Reference.Api.Controllers;

/// <summary>Product sections of the Calculator sheet «Типи і розділи».</summary>
[ApiController]
[Route("api/reference/product-sections")]
public sealed class ProductSectionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(List<ProductSectionRowDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProductSectionRowDTO>>> Get(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetProductSectionsQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = "access:product-sections")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<int>> Create(
        [FromBody] CreateProductSectionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductSectionCommand(new CreateProductSectionCommandRequest(
            request.Type, request.Name, request.IsActive, request.Comment, request.SortOrder));

        return this.ToActionResult(await sender.Send(command, cancellationToken));
    }

    /// <summary>
    /// Imports the sheet «Типи і розділи»: dryRun returns the preview; otherwise the rows are saved when none has
    /// an error.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Policy = "access:product-sections")]
    [ProducesResponseType(typeof(ProductSectionImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProductSectionImportResult>> Import(
        [FromBody] ImportProductSectionsRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(
            new ImportProductSectionsCommand(request.Rows, request.DryRun, request.DeleteMissing), cancellationToken));

    [HttpPut("{id:int}")]
    [Authorize(Policy = "access:product-sections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateProductSectionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductSectionCommand(id, new UpdateProductSectionCommandRequest(
            request.Type, request.Name, request.IsActive, request.Comment, request.SortOrder));

        return this.ToActionResult(await sender.Send(command, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "access:product-sections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new DeleteProductSectionCommand(id), cancellationToken));
}
