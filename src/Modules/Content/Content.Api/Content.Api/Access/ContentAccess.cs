namespace Content.Api.Access;

internal static class ContentAccess
{
    /// <summary>«Контент → Товари» on «Права доступу» (CNT-002 R-10): Admin all, Manager view by default.</summary>
    public const string ProductsPolicy = "access:content-products";

    /// <summary>«Контент → Розділи» on «Права доступу» (CNT-003 R-05): Admin all, Manager view by default.</summary>
    public const string CategoriesPolicy = "access:content-categories";

    /// <summary>«Контент → Статті» on «Права доступу» (CNT-004 R-04): Admin all, Manager view by default.</summary>
    public const string ArticlesPolicy = "access:content-articles";
}
