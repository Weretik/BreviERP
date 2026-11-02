namespace Reference.Infrastructure.KeepinCrm;

/// <summary>
/// Section "KeepinCrm". The API key comes from Secret Manager as environment variable
/// KeepinCrm__ApiKey and is never logged.
/// </summary>
public sealed class KeepinCrmOptions
{
    public const string Section = "KeepinCrm";

    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.keepincrm.com/v1/";

    /// <summary>KeepinCRM returns 25 suppliers per page; a shorter page is the last one.</summary>
    public int PageSize { get; set; } = 25;
    public int MaxPages { get; set; } = 100;
}
