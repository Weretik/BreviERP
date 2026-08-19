namespace Catalog.Api.Options;

public sealed class CatalogMediaUploadOptions
{
    public const string SectionName = "CatalogMediaUpload";

    public long MaxFileSizeBytes { get; init; }
    public required string BaseFolder { get; init; }
}
