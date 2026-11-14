namespace Catalog.Application.Features.Product.Commands.ImportOldSitePhotos;

/// <summary>
/// Photos of the old OpenCart site for Brevi products (CAT-016). A product is found by a sale SKU of the old-site
/// «model» (article SKUs of a sewing model, the site article of PPE), or by <see cref="OldSiteProduct.NewModel"/> name.
/// A missing sewing model with <see cref="OldSiteProduct.NewModel"/> is created switched off. Photos are downloaded
/// from <paramref name="BaseUrl"/> + path into the library folder «Старий сайт» and added after the product's own
/// photos; the old main photo becomes main only for a product without photos. A repeat adds nothing twice.
/// <paramref name="DryRun"/> only reports and downloads nothing; a real run takes at most
/// <see cref="MaxProductsPerRun"/> products, so the admin page sends the file in parts.
/// </summary>
public sealed record ImportOldSitePhotosCommand(
    bool DryRun,
    string BaseUrl,
    IReadOnlyList<OldSiteProduct> Products,
    string Actor) : ICommand<Result<OldSitePhotoImportReport>>
{
    public const int MaxProductsPerRun = 20;
    public const int MaxProductsPerDryRun = 2000;
    public const string SourcePrefix = "old-site:";
    public const string RootFolder = "Старий сайт";
}

/// <param name="Skus">Sale SKUs from the old-site «model» (for example 758 and 759 of «758-759»).</param>
public sealed record OldSiteProduct(
    int OldId,
    string OldName,
    IReadOnlyList<string> Skus,
    IReadOnlyList<OldSitePhoto> Photos,
    OldSiteNewModel? NewModel);

/// <param name="Path">Path under the old site image folder, for example catalog/costum_leto/a.png.</param>
public sealed record OldSitePhoto(string Path, bool IsMain);

/// <param name="Section">Name of a sewing section of «Типи і розділи».</param>
public sealed record OldSiteNewModel(string Name, string Section, string? Season);

/// <summary>What the run did, or would do for a dry run, with one line per product.</summary>
public sealed record OldSitePhotoImportReport(
    bool DryRun,
    int Products,
    int Found,
    int ModelsToCreate,
    int ModelsCreated,
    int NotFound,
    int FilesToDownload,
    int FilesDownloaded,
    int PhotosAdded,
    IReadOnlyList<OldSitePhotoImportLine> Lines);

/// <param name="Status">Found, Create (dry run), Created, NotFound.</param>
/// <param name="PhotosToAdd">Old photos the product does not have yet.</param>
/// <param name="Errors">Photos that failed, with the reason.</param>
public sealed record OldSitePhotoImportLine(
    int OldId,
    string OldName,
    string Status,
    int? ProductId,
    string? ProductName,
    int PhotosToAdd,
    int PhotosAdded,
    IReadOnlyList<string> Errors);
