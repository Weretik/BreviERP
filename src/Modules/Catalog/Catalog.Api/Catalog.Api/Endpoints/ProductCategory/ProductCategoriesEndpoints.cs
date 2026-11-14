namespace Catalog.Api.Endpoints.ProductCategory;

internal static class ProductCategoriesEndpoints
{
    public static IEndpointRouteBuilder MapProductCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reference/product-categories").RequireAuthorization("access:product-sections")
            .WithTags("ProductCategories");
        GetAdminProductCategoriesEndpoint.Map(group);
        GetStoreProductCategoriesEndpoint.Map(group);
        CreateProductCategoryEndpoint.Map(group);
        UpdateProductCategoryEndpoint.Map(group);
        DeleteProductCategoryEndpoint.Map(group);
        return app;
    }
}
