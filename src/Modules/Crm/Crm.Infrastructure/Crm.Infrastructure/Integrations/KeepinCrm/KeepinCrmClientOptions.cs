namespace Crm.Infrastructure.Integrations.KeepinCrm;

/// <summary>
/// Section "KeepinCrm", shared with the supplier sync. The API key comes from Secret Manager as environment variable
/// KeepinCrm__ApiKey and is never logged.
/// </summary>
public sealed class KeepinCrmClientOptions
{
    public const string Section = "KeepinCrm";

    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.keepincrm.com/v1/";

    /// <summary>Safety stop for paging through /clients; KeepinCRM reports the real count in pagination.total_pages.</summary>
    public int ClientMaxPages { get; set; } = 1000;

    /// <summary>Brevi user that owns clients created from KeepinCRM.</summary>
    public int ClientOwnerUserId { get; set; } = 1;
}
