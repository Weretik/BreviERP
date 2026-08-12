using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Media.Entities;
using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.ProductCategories.ValueObjects;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public class Product : BaseAuditableEntity<ProductId>, IAggregateRoot
{
    private const int NameMaxLength = 200;
    private readonly List<ProductPhoto> _photos = [];
    private readonly List<ProductCategoryReference> _categories = [];
    private readonly List<ProductInformationBlock> _informationBlocks = [];
    private readonly List<ProductCharacteristicTable> _characteristicTables = [];

    public string Name { get; private set; } = null!;
    public string RuName { get; private set; } = null!;
    public ProductSlug Slug { get; private set; }
    public ProductType Type { get; private set; }
    public string DescriptionUk { get; private set; } = string.Empty;
    public string DescriptionRu { get; private set; } = string.Empty;
    public SewingProductDetails? SewingDetails { get; private set; }
    public PpeProductDetails? PpeDetails { get; private set; }
    public IReadOnlyCollection<ProductPhoto> Photos => _photos.AsReadOnly();
    public IReadOnlyCollection<ProductCategoryReference> Categories => _categories.AsReadOnly();
    public IReadOnlyCollection<ProductInformationBlock> InformationBlocks => _informationBlocks.AsReadOnly();
    public IReadOnlyCollection<ProductCharacteristicTable> CharacteristicTables => _characteristicTables.AsReadOnly();

    private Product() { }

    private Product(ProductId id, string name, string ruName, ProductSlug slug, ProductType type, DateTimeOffset createdAt)
    {
        SetId(id);
        SetName(name);
        SetRuName(ruName);
        SetSlug(slug);
        SetType(type);
        MarkAsCreated(createdAt);
    }

    public static Product Create(ProductId id, string name, string ruName, ProductSlug slug, ProductType type, DateTimeOffset createdAt)
        => new(id, name, ruName, slug, type, createdAt);

    public void Update(string name, string ruName, ProductSlug slug, ProductType type, DateTimeOffset updatedAt)
    {
        SetName(name);
        SetRuName(ruName);
        SetSlug(slug);
        SetType(type);
        ClearDetailsForOtherType();
        MarkAsUpdated(updatedAt);
    }

    public void SetDescriptions(string descriptionUk, string descriptionRu, DateTimeOffset updatedAt)
    {
        DescriptionUk = NormalizeDescription(descriptionUk);
        DescriptionRu = NormalizeDescription(descriptionRu);
        MarkAsUpdated(updatedAt);
    }

    public void ReplaceContent(IEnumerable<ProductInformationBlock> informationBlocks,
        IEnumerable<ProductCharacteristicTable> characteristicTables, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(informationBlocks);
        ArgumentNullException.ThrowIfNull(characteristicTables);
        var blocks = informationBlocks.ToList();
        var tables = characteristicTables.ToList();
        if (blocks.Any(x => x.ProductId != Id) || tables.Any(x => x.ProductId != Id))
            throw new DomainException(ProductErrors.ContentBelongsToAnotherProduct());
        _informationBlocks.Clear();
        _informationBlocks.AddRange(blocks);
        _characteristicTables.Clear();
        _characteristicTables.AddRange(tables);
        MarkAsUpdated(updatedAt);
    }

    public void ConfigureSewing(SewingProductDetails details, DateTimeOffset updatedAt)
    {
        if (Type != ProductType.Sewing) throw new DomainException(ProductErrors.TypeSpecificDataInvalid());
        ArgumentNullException.ThrowIfNull(details);
        if (details.ProductId != Id) throw new DomainException(ProductErrors.DetailsBelongToAnotherProduct());
        SewingDetails = details;
        PpeDetails = null;
        MarkAsUpdated(updatedAt);
    }

    public void ConfigurePpe(PpeProductDetails details, DateTimeOffset updatedAt)
    {
        if (Type != ProductType.Ppe) throw new DomainException(ProductErrors.TypeSpecificDataInvalid());
        ArgumentNullException.ThrowIfNull(details);
        if (details.ProductId != Id) throw new DomainException(ProductErrors.DetailsBelongToAnotherProduct());
        PpeDetails = details;
        SewingDetails = null;
        MarkAsUpdated(updatedAt);
    }

    public void AddCategory(ProductCategoryId categoryId, DateTimeOffset updatedAt)
    {
        EnsureCategoryNotAttached(categoryId);
        _categories.Add(ProductCategoryReference.Create(Id, categoryId));
        MarkAsUpdated(updatedAt);
    }

    public void RemoveCategory(ProductCategoryId categoryId, DateTimeOffset updatedAt)
    {
        var category = GetCategory(categoryId);
        _categories.Remove(category);
        MarkAsUpdated(updatedAt);
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

    public void AddPhoto(
        ProductPhotoId photoId,
        MediaFileId mediaFileId,
        DateTimeOffset updatedAt,
        string? alt = null,
        bool isVisible = true,
        int sortOrder = 0,
        bool isMain = false)
    {
        if (_photos.Any(x => x.Id == photoId))
            throw new DomainException(ProductErrors.PhotoAlreadyExists(photoId.Value));

        EnsureMediaFileNotAttached(mediaFileId);

        var shouldBeMain = isMain || !_photos.Any();
        if (shouldBeMain)
            ClearMainPhoto();

        _photos.Add(ProductPhoto.Create(Id, photoId, mediaFileId, alt, isVisible, sortOrder, shouldBeMain));
        MarkAsUpdated(updatedAt);
    }

    public void AddPhoto(
        ProductPhotoId photoId,
        MediaFile mediaFile,
        DateTimeOffset updatedAt,
        string? alt = null,
        bool isVisible = true,
        int sortOrder = 0,
        bool isMain = false)
    {
        EnsureMediaFileReady(mediaFile);

        AddPhoto(photoId, mediaFile.Id, updatedAt, alt, isVisible, sortOrder, isMain);
    }

    public void SetPhotoVisibility(ProductPhotoId photoId, bool isVisible, DateTimeOffset updatedAt)
    {
        var photo = GetPhoto(photoId);
        photo.SetVisibility(isVisible);
        MarkAsUpdated(updatedAt);
    }

    public void SetPhotoAltText(ProductPhotoId photoId, string? alt, DateTimeOffset updatedAt)
    {
        var photo = GetPhoto(photoId);
        photo.SetAltText(alt);
        MarkAsUpdated(updatedAt);
    }

    public void ReplacePhotoMediaFile(ProductPhotoId photoId, MediaFileId mediaFileId, DateTimeOffset updatedAt)
    {
        var photo = GetPhoto(photoId);
        EnsureMediaFileNotAttached(mediaFileId, photoId);
        photo.ReplaceMediaFile(mediaFileId);
        MarkAsUpdated(updatedAt);
    }

    public void ReplacePhotoMediaFile(ProductPhotoId photoId, MediaFile mediaFile, DateTimeOffset updatedAt)
    {
        EnsureMediaFileReady(mediaFile);

        ReplacePhotoMediaFile(photoId, mediaFile.Id, updatedAt);
    }

    public void SetPhotoSortOrder(ProductPhotoId photoId, int sortOrder, DateTimeOffset updatedAt)
    {
        var photo = GetPhoto(photoId);
        photo.SetSortOrder(sortOrder);
        MarkAsUpdated(updatedAt);
    }

    public void SetMainPhoto(ProductPhotoId photoId, DateTimeOffset updatedAt)
    {
        var photo = GetPhoto(photoId);

        ClearMainPhoto();
        photo.SetMain(true);
        MarkAsUpdated(updatedAt);
    }

    public void RemovePhoto(ProductPhotoId photoId, DateTimeOffset updatedAt)
    {
        var photo = GetPhoto(photoId);
        var removedMainPhoto = photo.IsMain;
        _photos.Remove(photo);

        if (removedMainPhoto)
            EnsureMainPhotoSelected();
        MarkAsUpdated(updatedAt);
    }

    private void SetId(ProductId id)
    {
        if (id.Value == default)
            throw new DomainException(ProductErrors.IdIsRequired());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProductErrors.NameIsRequired());

        var normalizedName = name.Trim();
        if (normalizedName.Length > NameMaxLength)
            throw new DomainException(ProductErrors.NameLengthInvalid(NameMaxLength));

        Name = normalizedName;
    }

    private void SetRuName(string ruName)
    {
        if (string.IsNullOrWhiteSpace(ruName))
            throw new DomainException(ProductErrors.NameIsRequired());

        var normalizedRuName = ruName.Trim();
        if (normalizedRuName.Length > NameMaxLength)
            throw new DomainException(ProductErrors.NameLengthInvalid(NameMaxLength));

        RuName = normalizedRuName;
    }

    private void SetSlug(string slug)
    {
        Slug = ProductSlug.Create(slug);
    }

    private void SetType(ProductType type)
    {
        if (!Enum.IsDefined(type))
            throw new DomainException(ProductErrors.TypeIsRequired());

        Type = type;
    }

    private void ClearDetailsForOtherType()
    {
        if (Type == ProductType.Sewing) PpeDetails = null;
        else SewingDetails = null;
    }

    private static string NormalizeDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainException(ProductErrors.DescriptionIsRequired());
        var normalized = value.Trim();
        if (normalized.Length > 20_000) throw new DomainException(ProductErrors.DescriptionLengthInvalid());
        return normalized;
    }


    private ProductPhoto GetPhoto(ProductPhotoId photoId)
    {
        var photo = _photos.FirstOrDefault(x => x.Id == photoId);
        if (photo is null)
            throw new DomainException(ProductErrors.PhotoNotFound(photoId.Value));

        return photo;
    }

    private ProductCategoryReference GetCategory(ProductCategoryId categoryId)
    {
        var category = _categories.FirstOrDefault(x => x.CategoryId == categoryId);
        if (category is null)
            throw new DomainException(ProductErrors.CategoryNotFound(categoryId.Value));

        return category;
    }

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

    private void EnsureMediaFileNotAttached(MediaFileId mediaFileId, ProductPhotoId? exceptPhotoId = null)
    {
        var alreadyAttached = _photos.Any(x =>
            x.MediaFileId == mediaFileId &&
            (!exceptPhotoId.HasValue || x.Id != exceptPhotoId.Value));

        if (alreadyAttached)
            throw new DomainException(ProductErrors.PhotoMediaFileAlreadyAttached(mediaFileId.Value));
    }

    private void EnsureCategoryNotAttached(ProductCategoryId categoryId)
    {
        if (categoryId.Value == default)
            throw new DomainException(ProductErrors.CategoryIdIsRequired());

        var alreadyAttached = _categories.Any(x => x.CategoryId == categoryId);
        if (alreadyAttached)
            throw new DomainException(ProductErrors.CategoryAlreadyAttached(categoryId.Value));
    }

    private static void EnsureMediaFileReady(MediaFile mediaFile)
    {
        ArgumentNullException.ThrowIfNull(mediaFile);

        if (!mediaFile.IsReadyForProductUsage())
            throw new DomainException(ProductErrors.PhotoMediaFileNotReady(mediaFile.Id.Value));
    }
}
