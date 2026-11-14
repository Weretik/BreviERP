using Catalog.Application.Contracts.Media;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Product.Overview.Create;
using Catalog.Application.Features.Product.Overview.Shared;
using Catalog.Application.Features.Product.Photos.DTOs;
using Catalog.Application.Features.Product.Photos.SetPhotos;
using Catalog.Application.Features.Product.Photos.SkuIndex;
using Catalog.Application.Features.Product.Photos.Specifications;
using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.OldSitePhotoImport;

public sealed class ImportOldSitePhotosCommandHandler(
    ISender sender,
    ICatalogReadRepository<ProductEntity> products,
    IProductSectionReader sections,
    IProductPhotoSource photoSource) : ICommandHandler<ImportOldSitePhotosCommand, Result<OldSitePhotoImportReport>>
{
    public const string Found = "Found";
    public const string Create = "Create";
    public const string Created = "Created";
    public const string NotFound = "NotFound";

    public async ValueTask<Result<OldSitePhotoImportReport>> Handle(
        ImportOldSitePhotosCommand command, CancellationToken cancellationToken)
    {
        var index = (await sender.Send(new GetProductSkuIndexQuery(), cancellationToken)).Value;
        var bySku = new Dictionary<string, ProductSkuIndexItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in index)
            foreach (var sku in item.Skus)
                bySku.TryAdd(sku, item);
        var byName = index.Where(x => x.Type == ProductOverviewTypes.Name(ProductType.Sewing))
            .GroupBy(x => Key(x.Name)).ToDictionary(x => x.Key, x => x.First());
        var sewingSections = (await sections.ListAsync(cancellationToken))
            .Where(x => x.Type == ProductOverviewTypes.Name(ProductType.Sewing))
            .GroupBy(x => Key(x.Name)).ToDictionary(x => x.Key, x => x.First());
        var imported = new Dictionary<string, int>(await photoSource.FindImportedAsync(
            command.Products.SelectMany(x => x.Photos).Select(x => OldSitePhotoPaths.SourceKey(x.Path)).Distinct().ToList(),
            cancellationToken));

        var lines = new List<OldSitePhotoImportLine>();
        var toDownload = new HashSet<string>();
        var downloaded = 0;
        foreach (var old in command.Products)
        {
            var errors = new List<string>();
            var product = old.Skus.Select(sku => bySku.GetValueOrDefault(sku.Trim())).FirstOrDefault(x => x is not null)
                          ?? (old.NewModel is { } named ? byName.GetValueOrDefault(Key(named.Name)) : null);
            var status = product is null ? NotFound : Found;
            int? productId = product?.ProductId;
            var productName = product?.Name;

            if (product is null && old.NewModel is { } model)
            {
                if (!sewingSections.TryGetValue(Key(model.Section), out var section))
                    errors.Add($"Розділ «{model.Section}» не знайдено в «Типи і розділи».");
                else if (command.DryRun)
                {
                    status = Create;
                    productName = model.Name.Trim();
                }
                else
                {
                    var created = await sender.Send(new CreateCatalogProductCommand(
                        ProductOverviewTypes.Name(ProductType.Sewing), null, model.Name.Trim(), section.Id,
                        string.IsNullOrWhiteSpace(model.Season) ? null : model.Season.Trim(), false, null, null, null,
                        command.Actor), cancellationToken);
                    if (created.IsSuccess)
                    {
                        status = Created;
                        productId = created.Value.Id;
                        productName = model.Name.Trim();
                        byName[Key(model.Name)] = new ProductSkuIndexItem(created.Value.Id,
                            ProductOverviewTypes.Name(ProductType.Sewing), productName, false, []);
                    }
                    else errors.Add("Модель не створено: " + Describe(created));
                }
            }

            var current = productId is { } id ? await CurrentPhotosAsync(id, cancellationToken) : [];
            var photos = old.Photos.OrderByDescending(x => x.IsMain)
                .DistinctBy(x => x.Path.Trim(), StringComparer.Ordinal).ToList();
            var missing = photos.Where(x => !imported.TryGetValue(OldSitePhotoPaths.SourceKey(x.Path), out var fileId)
                                            || !current.Contains(fileId)).ToList();
            foreach (var photo in photos.Where(x => !imported.ContainsKey(OldSitePhotoPaths.SourceKey(x.Path))))
                toDownload.Add(photo.Path.Trim());

            var added = 0;
            if (!command.DryRun && productId is { } target && missing.Count > 0)
            {
                var ordered = new List<int>();
                int? oldMain = null;
                foreach (var photo in photos)
                {
                    var key = OldSitePhotoPaths.SourceKey(photo.Path);
                    if (!imported.TryGetValue(key, out var fileId))
                    {
                        var download = await photoSource.DownloadAsync(OldSitePhotoPaths.Url(command.BaseUrl, photo.Path),
                            key, OldSitePhotoPaths.Folder(photo.Path), cancellationToken);
                        if (download.MediaFileId is not { } newId)
                        {
                            errors.Add($"{photo.Path}: {download.Error}");
                            continue;
                        }

                        fileId = newId;
                        imported[key] = newId;
                        downloaded++;
                    }

                    ordered.Add(fileId);
                    if (photo.IsMain) oldMain ??= fileId;
                }

                var fresh = ordered.Where(x => !current.Contains(x)).ToList();
                if (fresh.Count > 0)
                {
                    var main = current.Count == 0 ? oldMain ?? fresh[0] : (int?)null;
                    var saved = await sender.Send(new SetProductPhotosCommand(target, [.. current, .. fresh], main),
                        cancellationToken);
                    if (saved.IsSuccess) added = fresh.Count;
                    else errors.Add("Фото не додано до товару: " + Describe(saved));
                }
            }

            lines.Add(new OldSitePhotoImportLine(old.OldId, old.OldName, status, productId, productName,
                missing.Count, added, errors));
        }

        return Result.Success(new OldSitePhotoImportReport(
            command.DryRun,
            lines.Count,
            lines.Count(x => x.Status == Found),
            lines.Count(x => x.Status == Create),
            lines.Count(x => x.Status == Created),
            lines.Count(x => x.Status == NotFound),
            toDownload.Count,
            downloaded,
            lines.Sum(x => x.PhotosAdded),
            lines));
    }

    private async Task<List<int>> CurrentPhotosAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await products.FirstOrDefaultAsync(new ProductPhotosByIdSpec(productId, false), cancellationToken);
        return product?.Photos.OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value).Select(x => x.MediaFileId.Value).ToList() ?? [];
    }

    private static string Key(string value) => string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();

    private static string Describe(IResult result)
    {
        var validation = result.ValidationErrors?.Select(x => x.ErrorMessage) ?? [];
        var messages = validation.Concat(result.Errors ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        return messages.Count == 0 ? result.Status.ToString() : string.Join("; ", messages);
    }
}
