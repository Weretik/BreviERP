using Reference.Api.Endpoints.ProductSections.CreateProductSection;
using Reference.Api.Endpoints.ProductSections.DeleteProductSection;
using Reference.Api.Endpoints.ProductSections.GetProductSections;
using Reference.Api.Endpoints.ProductSections.ImportProductSections;
using Reference.Api.Endpoints.ProductSections.UpdateProductSection;

namespace Reference.Api.Endpoints.ProductSections;

/// <summary>Product sections of the Calculator sheet «Типи і розділи».</summary>
internal static class ProductSectionsEndpoints
{
    public static IEndpointRouteBuilder MapProductSectionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reference/product-sections").WithTags("ProductSections");
        GetProductSectionsEndpoint.Map(group);
        CreateProductSectionEndpoint.Map(group);
        ImportProductSectionsEndpoint.Map(group);
        UpdateProductSectionEndpoint.Map(group);
        DeleteProductSectionEndpoint.Map(group);
        return app;
    }
}
