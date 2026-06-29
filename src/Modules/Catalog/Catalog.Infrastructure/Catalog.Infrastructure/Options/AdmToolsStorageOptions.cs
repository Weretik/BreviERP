namespace Catalog.Infrastructure.Options;

public sealed class AdmToolsStorageOptions
{
    public const string SectionName = "AdmToolsStorage";

    public string Host { get; init; } = string.Empty;
    public string Login { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string PublicBaseUrl { get; init; } = string.Empty;
}
