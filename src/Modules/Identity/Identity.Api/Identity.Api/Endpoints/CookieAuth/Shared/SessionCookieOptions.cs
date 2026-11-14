namespace Identity.Api.Endpoints.CookieAuth.Shared;

/// <summary>
/// The readable CSRF cookie that pairs with the HttpOnly session cookie.
/// </summary>
public sealed class SessionCookieOptions
{
    public const string SectionName = "Identity:SessionCookies";

    public string CsrfCookieName { get; init; } = "brevi.csrf";
    public string CsrfHeaderName { get; init; } = "X-CSRF-Token";
    public string CsrfCookiePath { get; init; } = "/";
    // Shared parent domain (brevi.com.ua) so Admin can read the cookie; empty keeps it host-only.
    public string? CsrfCookieDomain { get; init; }
    public int CsrfCookieTtlDays { get; init; } = 14;

    public bool Secure { get; init; } = true;
    public SameSiteMode SameSite { get; init; } = SameSiteMode.Lax;
}
