using Catalog.Application.Features.Product.Commands.CreateCatalogProduct;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Overview.CreateCatalogProduct;

internal static class CreateCatalogProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("overview", HandleAsync).WithName("createCatalogProduct")
            .WithSummary("Create a product from the page «Новий ЗІЗ» or «Нова модель»")
            .WithDescription("Requires access:products and an actor (CAT-009). The server picks the ID (largest + 1) and the slug (from the Ukrainian name; the code and then a number are added when it is taken). The name is Ukrainian only. An empty sewing code takes the next free MOD-XXX. The section must belong to the type. A PPE product needs the ppe block; the first price version is appended with source «Створення ЗІЗ». priceSaved is false when the product was saved but its price was not; the card then asks to save the price again.")
            .RequireAuthorization("access:products")
            .Produces<CreatedCatalogProduct>(StatusCodes.Status201Created);

    private static async Task<IResult> HandleAsync(CreateCatalogProductRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        var ppe = request.Ppe is { } p
            ? new CreatePpeSupplierInput(p.SupplierId, p.SupplierSku, p.SupplierUrl, p.SupplierPrice,
                p.AllowManualPrice, p.RuleCode, p.Unit)
            : null;
        var result = await sender.Send(new CreateCatalogProductCommand(request.Type, request.Code, request.Name,
            request.SectionId, request.Season, request.IsActive, request.CurrentSiteUrl,
            request.QuoteDescription, ppe, actor), cancellationToken);
        return result.IsSuccess
            ? Results.Json(statusCode: StatusCodes.Status201Created, data: result.Value)
            : result.ToHttpResult();
    }
}
