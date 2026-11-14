using Catalog.Api.Product.Overview;
using Catalog.Application.Features.Product.Commands.CreateCatalogProduct;

namespace Catalog.Api.Endpoints.Overview;

internal static class CreateCatalogProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("overview", HandleAsync).WithName("createCatalogProduct")
            .RequireAuthorization("access:products");

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
