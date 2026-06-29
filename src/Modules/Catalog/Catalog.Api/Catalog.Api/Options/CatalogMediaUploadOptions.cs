namespace Catalog.Api.Options;

public sealed class CatalogMediaUploadOptions
{
    public const string SectionName = "CatalogMediaUpload";

    public long MaxFileSizeBytes { get; init; } = 50L * 1024 * 1024;
    public string BaseFolder { get; init; } = "/products/original";
}
