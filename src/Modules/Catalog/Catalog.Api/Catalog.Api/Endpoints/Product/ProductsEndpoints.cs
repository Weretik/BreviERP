using Catalog.Api.Endpoints.Product.CreateProduct;
using Catalog.Api.Endpoints.Product.DeleteProduct;
using Catalog.Api.Endpoints.Product.GetProduct;
using Catalog.Api.Endpoints.Product.GetProducts;
using Catalog.Api.Endpoints.Product.ReplaceProduct;

namespace Catalog.Api.Endpoints.Product;

internal static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/products").RequireAuthorization("access:products")
            .WithTags("Products");
        GetProductsEndpoint.Map(group);
        GetProductEndpoint.Map(group);
        CreateProductEndpoint.Map(group);
        ReplaceProductEndpoint.Map(group);
        DeleteProductEndpoint.Map(group);
        return app;
    }
}
