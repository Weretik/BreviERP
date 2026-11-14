using Content.Api.Access;

namespace Content.Api.Endpoints.SiteCategories;

/// <summary>«Контент → Розділи»: categories and subcategories of the public site catalog (CNT-003).</summary>
internal static class SiteCategoriesEndpoints
{
    public static IEndpointRouteBuilder MapSiteCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/content/categories").RequireAuthorization(ContentAccess.CategoriesPolicy)
            .WithTags("SiteCategories");
        GetSiteCategoriesEndpoint.Map(group);
        GetSiteCategoryEndpoint.Map(group);
        CreateSiteCategoryEndpoint.Map(group);
        SaveSiteCategoryEndpoint.Map(group);
        SetSiteCategoryVisibilityEndpoint.Map(group);
        ReorderSiteCategoriesEndpoint.Map(group);
        ImportOldSiteCategoriesEndpoint.Map(group);
        DeleteSiteCategoryEndpoint.Map(group);
        return app;
    }
}
