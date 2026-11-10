namespace Content.Api.Access;

internal static class ContentAccess
{
    /// <summary>«Контент → Товари» on «Права доступу» (CNT-002 R-10): Admin all, Manager view by default.</summary>
    public const string ProductsPolicy = "access:content-products";
}
