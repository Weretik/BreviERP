using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.ProductCategory.Shared.Specifications;
using Catalog.Application.Features.Product.GetAdminDetail.DTOs;
using Catalog.Application.Features.Product.GetAdminList.Specifications;
using Catalog.Application.Features.Product.GetAdminList.DTOs;
using Catalog.Application.Contracts.Admin.Product;
using Catalog.Application.Contracts.Reference;
using Catalog.Domain.Media.Entities;
using Catalog.Domain.ProductCategories.ValueObjects;
using Catalog.Domain.Products.ValueObjects;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.GetAdminList;

public sealed class GetAdminProductsQueryHandler(
    ICatalogReadRepository<ProductEntity> repository,
    ICatalogReadRepository<ProductCategoryEntity> categoryRepository,
    ICatalogReadRepository<MediaFile> mediaRepository,
    IProductListPricingReferenceReader pricingReferenceReader) : IQueryHandler<GetAdminProductsQuery, PagedResult<IReadOnlyList<ProductListItem>>>
{
    public async ValueTask<PagedResult<IReadOnlyList<ProductListItem>>> Handle(GetAdminProductsQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page;
        var pageSize = query.PageSize;
        IReadOnlyCollection<ProductCategoryId>? categoryIds = null;

        if (query.CategoryId.HasValue)
        {
            var category = await categoryRepository.FirstOrDefaultAsync(
                new ProductCategoryByIdSpec(query.CategoryId.Value), cancellationToken);
            if (category is null)
                return CreatePagedResult([], page, pageSize, 0);

            var descendants = await categoryRepository.ListAsync(
                new ProductCategoryDescendantsByPathSpec(category.Id.Value, category.Path), cancellationToken);
            categoryIds = [category.Id, .. descendants.Select(x => x.Id)];
        }

        var filter = new GetAdminProductsSpec(query.Search, query.Type, categoryIds, query.SortBy, query.Descending);
        var total = await repository.CountAsync(filter, cancellationToken);
        var rows = await repository.ListAsync(
            new GetAdminProductsSpec(query.Search, query.Type, categoryIds, query.SortBy, query.Descending, page, pageSize),
            cancellationToken);

        if (rows.Count == 0)
            return CreatePagedResult([], page, pageSize, total);

        var mainPhotoIds = rows
            .Select(x => x.MainPhotoMediaFileId)
            .OfType<int>()
            .Distinct()
            .ToArray();
        var mediaUrls = mainPhotoIds.Length == 0
            ? new Dictionary<int, string>()
            : (await mediaRepository.ListAsync(new ProductMediaUrlsByIdsSpec(mainPhotoIds), cancellationToken))
                .ToDictionary(x => x.MediaFileId, x => x.Url);
        var references = await GetPricingReferencesAsync(rows, cancellationToken);
        var items = rows.Select(row => new ProductListItem(
            row.Id,
            row.Name,
            row.Slug,
            row.Type,
            row.CategoryIds,
            row.MainPhotoMediaFileId is { } mediaFileId && mediaUrls.TryGetValue(mediaFileId, out var url)
                ? new ProductMainPhoto(mediaFileId, url)
                : null,
            MinimumWholesalePrice(row, references),
            row.CreatedAtUtc,
            row.UpdatedAtUtc)).ToList();

        return CreatePagedResult(items, page, pageSize, total);
    }

    private async Task<ProductReferenceData> GetPricingReferencesAsync(
        IReadOnlyCollection<ProductListItemReadModel> rows,
        CancellationToken cancellationToken)
    {
        var sewing = rows.Select(x => x.Sewing).OfType<SewingReadModel>().ToArray();
        var ppe = rows.Select(x => x.Ppe).OfType<PpeReadModel>().ToArray();
        var fabricIds = sewing.SelectMany(x => x.Fabrics).Select(x => x.FabricId).Distinct().ToArray();
        var accessoryIds = sewing.SelectMany(x => x.Accessories).Select(x => x.GarmentAccessoryId).Distinct().ToArray();
        var operationIds = sewing.SelectMany(x => x.OperationIds).Distinct().ToArray();
        var additionalReferenceIds = ppe
            .SelectMany(x => new[]
            {
                x.RetailSource == PricePercentSource.Reference ? x.RetailAdditionalReferenceId : null,
                x.WholesaleSource == PricePercentSource.Reference ? x.WholesaleAdditionalReferenceId : null
            })
            .OfType<int>()
            .Distinct()
            .ToArray();
        var additionalReferenceKeys = fabricIds.Length == 0
            ? []
            : ProductAdminDetailMapper.SewingPricingReferenceKeys;

        if (fabricIds.Length == 0 && accessoryIds.Length == 0 && operationIds.Length == 0 &&
            additionalReferenceIds.Length == 0 && additionalReferenceKeys.Count == 0)
            return EmptyReferences();

        var pricing = await pricingReferenceReader.GetAsync(
            new ProductListPricingReferenceRequest(
                fabricIds,
                accessoryIds,
                operationIds,
                additionalReferenceIds,
                additionalReferenceKeys),
            cancellationToken);

        return new ProductReferenceData(
            new Dictionary<int, ReferenceItem>(),
            pricing.FabricPrices.ToDictionary(x => x.Key, x => new PricedReferenceItem(x.Key, string.Empty, x.Value)),
            pricing.GarmentAccessoryPrices.ToDictionary(x => x.Key, x => new PricedReferenceItem(x.Key, string.Empty, x.Value)),
            pricing.GarmentPartOperationMinutes.ToDictionary(x => x.Key, x => new OperationReferenceItem(x.Key, string.Empty, x.Value)),
            pricing.AdditionalReferences.ToDictionary(
                x => x.Key,
                x => new AdditionalReferenceItem(x.Value.Id, x.Value.Key, x.Value.Key, x.Value.Value, x.Value.Unit)));
    }

    private static ProductReferenceData EmptyReferences()
        => new(
            new Dictionary<int, ReferenceItem>(),
            new Dictionary<int, PricedReferenceItem>(),
            new Dictionary<int, PricedReferenceItem>(),
            new Dictionary<int, OperationReferenceItem>(),
            new Dictionary<int, AdditionalReferenceItem>());

    private static PagedResult<IReadOnlyList<ProductListItem>> CreatePagedResult(
        IReadOnlyList<ProductListItem> items,
        int page,
        int pageSize,
        int totalRecords)
        => new(
            new PagedInfo(page, pageSize, (long)Math.Ceiling(totalRecords / (double)pageSize), totalRecords),
            items);

    private static decimal MinimumWholesalePrice(ProductListItemReadModel product, ProductReferenceData references)
        => product.Type switch
        {
            Catalog.Domain.Products.Enums.ProductType.Sewing when product.Sewing is not null =>
                ProductAdminDetailMapper.CalculateSewingPrices(product.Sewing, references)?.ByFabric
                    .SelectMany(x => new[] { x.Price1To10, x.Price11To39, x.Price40Plus })
                    .DefaultIfEmpty(0m)
                    .Min() ?? 0m,
            Catalog.Domain.Products.Enums.ProductType.Ppe when product.Ppe is not null =>
                ProductAdminDetailMapper.CalculatePpeWholesalePrice(product.Ppe, references) ?? 0m,
            _ => 0m
        };
}
