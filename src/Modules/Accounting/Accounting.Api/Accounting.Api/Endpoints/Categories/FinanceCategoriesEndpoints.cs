using Accounting.Api.Access;

namespace Accounting.Api.Endpoints.Categories;

/// <summary>«Бухгалтерія → Категорії»: two levels per area of the books and direction.</summary>
internal static class FinanceCategoriesEndpoints
{
    public static IEndpointRouteBuilder MapFinanceCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/accounting/categories").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("FinanceCategories");
        GetFinanceCategoriesEndpoint.Map(group);
        CreateFinanceCategoryEndpoint.Map(group);
        UpdateFinanceCategoryEndpoint.Map(group);
        DeleteFinanceCategoryEndpoint.Map(group);
        GetFinanceCategoryHistoryEndpoint.Map(group);
        return app;
    }
}
