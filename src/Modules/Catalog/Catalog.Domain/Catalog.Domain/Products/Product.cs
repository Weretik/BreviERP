using BuildingBlocks.Domain.Entity;
using Catalog.Domain.Media;
using Catalog.Domain.ProductCategories;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.Photos;
using Catalog.Domain.Products.Sewing.Composition;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products;

public partial class Product : BaseAuditableEntity<ProductId>, IAggregateRoot
{
    private const int NameMaxLength = 200;
    private readonly List<ProductPhoto> _photos = [];
    private readonly List<ProductCategoryReference> _categories = [];
    private readonly List<ProductInformationBlock> _informationBlocks = [];
    private readonly List<ProductCharacteristicTable> _characteristicTables = [];
    private readonly List<SewingArticle> _articles = [];
    private readonly List<SewingMaterialLine> _materialLines = [];
    private readonly List<SewingOperationLine> _operationLines = [];

    public string Name { get; private set; } = null!;
    public string RuName { get; private set; } = null!;
    public ProductSlug Slug { get; private set; }
    public ProductType Type { get; private set; }
    public string DescriptionUk { get; private set; } = string.Empty;
    public string DescriptionRu { get; private set; } = string.Empty;
    public SewingProductDetails? SewingDetails { get; private set; }
    public string? ModelCode { get; private set; }
    public long CompositionVersion { get; private set; }
    public IReadOnlyCollection<SewingArticle> Articles => _articles.AsReadOnly();
    public IReadOnlyCollection<SewingMaterialLine> MaterialLines => _materialLines.AsReadOnly();
    public IReadOnlyCollection<SewingOperationLine> OperationLines => _operationLines.AsReadOnly();
    public PpeProductDetails? PpeDetails { get; private set; }
    public IReadOnlyCollection<ProductPhoto> Photos => _photos.AsReadOnly();
    public IReadOnlyCollection<ProductCategoryReference> Categories => _categories.AsReadOnly();
    public IReadOnlyCollection<ProductInformationBlock> InformationBlocks => _informationBlocks.AsReadOnly();
    public IReadOnlyCollection<ProductCharacteristicTable> CharacteristicTables => _characteristicTables.AsReadOnly();

    private Product() { }

    public static DomainResult<Product> Create(ProductId id, string name, string ruName, ProductSlug slug, ProductType type,
        DateTimeOffset createdAt)
    {
        if (id.Value == default) throw new ArgumentException("A product needs an id.", nameof(id));
        var product = new Product { Id = id };
        var applied = product.ApplyIdentity(name, ruName, slug, type);
        if (applied.IsFailure) return applied.Error!;
        product.MarkAsCreated(createdAt);
        return product;
    }

    public DomainResult Update(string name, string ruName, ProductSlug slug, ProductType type, DateTimeOffset updatedAt)
    {
        if (type == ProductType.Ppe && Type == ProductType.Sewing &&
            (_articles.Count != 0 || _materialLines.Count != 0 || _operationLines.Count != 0))
            return ProductErrors.TypeSpecificDataInvalid();
        var applied = ApplyIdentity(name, ruName, slug, type);
        if (applied.IsFailure) return applied;
        ClearDetailsForOtherType();
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult SetDescriptions(string descriptionUk, string descriptionRu, DateTimeOffset updatedAt)
    {
        var uk = NormalizeDescription(descriptionUk);
        if (uk.IsFailure) return uk.Error!;
        var ru = NormalizeDescription(descriptionRu);
        if (ru.IsFailure) return ru.Error!;
        DescriptionUk = uk.Value;
        DescriptionRu = ru.Value;
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult ReplaceContent(IEnumerable<ProductInformationBlock> informationBlocks,
        IEnumerable<ProductCharacteristicTable> characteristicTables, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(informationBlocks);
        ArgumentNullException.ThrowIfNull(characteristicTables);
        var blocks = informationBlocks.ToList();
        var tables = characteristicTables.ToList();
        if (blocks.Any(x => x.ProductId != Id) || tables.Any(x => x.ProductId != Id))
            return ProductErrors.ContentBelongsToAnotherProduct();
        _informationBlocks.Clear();
        _informationBlocks.AddRange(blocks);
        _characteristicTables.Clear();
        _characteristicTables.AddRange(tables);
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult ConfigureSewing(SewingProductDetails details, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(details);
        if (Type != ProductType.Sewing) return ProductErrors.TypeSpecificDataInvalid();
        if (details.ProductId != Id) return ProductErrors.DetailsBelongToAnotherProduct();
        SewingDetails = details;
        PpeDetails = null;
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult ConfigurePpe(PpeProductDetails details, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(details);
        if (Type != ProductType.Ppe) return ProductErrors.TypeSpecificDataInvalid();
        if (details.ProductId != Id) return ProductErrors.DetailsBelongToAnotherProduct();
        PpeDetails = details;
        SewingDetails = null;
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult AddCategory(ProductCategoryId categoryId, DateTimeOffset updatedAt)
    {
        if (_categories.Any(x => x.CategoryId == categoryId))
            return ProductErrors.CategoryAlreadyAttached(categoryId.Value);
        _categories.Add(ProductCategoryReference.Create(Id, categoryId));
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult RemoveCategory(ProductCategoryId categoryId, DateTimeOffset updatedAt)
    {
        var category = _categories.FirstOrDefault(x => x.CategoryId == categoryId);
        if (category is null) return ProductErrors.CategoryNotFound(categoryId.Value);
        _categories.Remove(category);
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public void ReplaceCategories(IEnumerable<ProductCategoryId> categoryIds, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(categoryIds);

        var uniqueCategoryIds = categoryIds.Distinct().ToList();

        _categories.Clear();
        foreach (var categoryId in uniqueCategoryIds)
            _categories.Add(ProductCategoryReference.Create(Id, categoryId));
        MarkAsUpdated(updatedAt);
    }

    public DomainResult AddPhoto(
        ProductPhotoId photoId,
        MediaFileId mediaFileId,
        DateTimeOffset updatedAt,
        string? alt = null,
        bool isVisible = true,
        int sortOrder = 0,
        bool isMain = false)
    {
        if (_photos.Any(x => x.Id == photoId))
            return ProductErrors.PhotoAlreadyExists(photoId.Value);
        if (CheckMediaFileNotAttached(mediaFileId) is { IsFailure: true } attached) return attached;

        var shouldBeMain = isMain || !_photos.Any();
        var photo = ProductPhoto.Create(Id, photoId, mediaFileId, alt, isVisible, sortOrder, shouldBeMain);
        if (photo.IsFailure) return photo.Error!;
        if (shouldBeMain)
            ClearMainPhoto();
        _photos.Add(photo.Value);
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult SetPhotoVisibility(ProductPhotoId photoId, bool isVisible, DateTimeOffset updatedAt)
    {
        if (FindPhoto(photoId) is not { } photo) return ProductErrors.PhotoNotFound(photoId.Value);
        photo.SetVisibility(isVisible);
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult SetPhotoAltText(ProductPhotoId photoId, string? alt, DateTimeOffset updatedAt)
    {
        if (FindPhoto(photoId) is not { } photo) return ProductErrors.PhotoNotFound(photoId.Value);
        var set = photo.SetAltText(alt);
        if (set.IsFailure) return set;
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult ReplacePhotoMediaFile(ProductPhotoId photoId, MediaFileId mediaFileId, DateTimeOffset updatedAt)
    {
        if (FindPhoto(photoId) is not { } photo) return ProductErrors.PhotoNotFound(photoId.Value);
        if (CheckMediaFileNotAttached(mediaFileId, photoId) is { IsFailure: true } attached) return attached;
        photo.ReplaceMediaFile(mediaFileId);
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult SetPhotoSortOrder(ProductPhotoId photoId, int sortOrder, DateTimeOffset updatedAt)
    {
        if (FindPhoto(photoId) is not { } photo) return ProductErrors.PhotoNotFound(photoId.Value);
        var set = photo.SetSortOrder(sortOrder);
        if (set.IsFailure) return set;
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult SetMainPhoto(ProductPhotoId photoId, DateTimeOffset updatedAt)
    {
        if (FindPhoto(photoId) is not { } photo) return ProductErrors.PhotoNotFound(photoId.Value);
        ClearMainPhoto();
        photo.SetMain(true);
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    public DomainResult RemovePhoto(ProductPhotoId photoId, DateTimeOffset updatedAt)
    {
        if (FindPhoto(photoId) is not { } photo) return ProductErrors.PhotoNotFound(photoId.Value);
        var removedMainPhoto = photo.IsMain;
        _photos.Remove(photo);

        if (removedMainPhoto)
            EnsureMainPhotoSelected();
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    /// <summary>Checks every identity field first, so a refused change leaves the product as it was.</summary>
    private DomainResult ApplyIdentity(string name, string ruName, ProductSlug slug, ProductType type)
    {
        if (slug.Value is null) throw new ArgumentException("A product needs a slug.", nameof(slug));
        var cleanName = CleanName(name);
        if (cleanName.IsFailure) return cleanName.Error!;
        var cleanRuName = CleanName(ruName);
        if (cleanRuName.IsFailure) return cleanRuName.Error!;
        if (!Enum.IsDefined(type)) return ProductErrors.TypeIsRequired();
        Name = cleanName.Value;
        RuName = cleanRuName.Value;
        Slug = slug;
        Type = type;
        return DomainResult.Success();
    }

    private static DomainResult<string> CleanName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return ProductErrors.NameIsRequired();
        var normalizedName = name.Trim();
        if (normalizedName.Length > NameMaxLength) return ProductErrors.NameLengthInvalid(NameMaxLength);
        return normalizedName;
    }

    private void ClearDetailsForOtherType()
    {
        if (Type == ProductType.Sewing) PpeDetails = null;
        else SewingDetails = null;
    }

    private static DomainResult<string> NormalizeDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return ProductErrors.DescriptionIsRequired();
        var normalized = value.Trim();
        if (normalized.Length > 20_000) return ProductErrors.DescriptionLengthInvalid();
        return normalized;
    }

    private ProductPhoto? FindPhoto(ProductPhotoId photoId) => _photos.FirstOrDefault(x => x.Id == photoId);

    private void ClearMainPhoto()
    {
        foreach (var photo in _photos.Where(x => x.IsMain))
            photo.SetMain(false);
    }

    private void EnsureMainPhotoSelected()
    {
        var nextMainPhoto = _photos
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id.Value)
            .FirstOrDefault();

        if (nextMainPhoto is not null)
            nextMainPhoto.SetMain(true);
    }

    private DomainResult CheckMediaFileNotAttached(MediaFileId mediaFileId, ProductPhotoId? exceptPhotoId = null)
    {
        var alreadyAttached = _photos.Any(x =>
            x.MediaFileId == mediaFileId &&
            (!exceptPhotoId.HasValue || x.Id != exceptPhotoId.Value));

        return alreadyAttached
            ? ProductErrors.PhotoMediaFileAlreadyAttached(mediaFileId.Value)
            : DomainResult.Success();
    }
}
