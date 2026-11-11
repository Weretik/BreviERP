namespace Content.Api.Access;

internal static class ContentAccess
{
    /// <summary>«Контент → Товари» on «Права доступу» (CNT-002 R-10): Admin all, Manager view by default.</summary>
    public const string ProductsPolicy = "access:content-products";

    /// <summary>«Контент → Розділи» on «Права доступу» (CNT-003 R-05): Admin all, Manager view by default.</summary>
    public const string CategoriesPolicy = "access:content-categories";

    /// <summary>«Контент → Статті» on «Права доступу» (CNT-004 R-04): Admin all, Manager view by default.</summary>
    public const string ArticlesPolicy = "access:content-articles";

    /// <summary>«Контент → Переадресації» on «Права доступу» (CNT-005 R-03): Admin all, Manager view by default.</summary>
    public const string RedirectsPolicy = "access:content-redirects";

    /// <summary>«Контент → Налаштування SEO сайту» on «Права доступу» (CNT-006 R-03): Admin view and edit, Manager view.</summary>
    public const string SeoSettingsPolicy = "access:content-seo-settings";
}
