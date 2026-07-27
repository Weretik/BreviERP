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

public class Product : BaseEntity<ProductId>, IAggregateRoot
{
    private const int NameMaxLength = 200;
    private readonly List<ProductPhoto> _photos = [];
    private readonly List<ProductCategoryReference> _categories = [];

    public string Name { get; private set; } = null!;
    public string RuName { get; private set; } = null!;
    public ProductSlug Slug { get; private set; }
    public ProductType Type { get; private set; }
    public IReadOnlyCollection<ProductPhoto> Photos => _photos.AsReadOnly();
    public IReadOnlyCollection<ProductCategoryReference> Categories => _categories.AsReadOnly();

    private Product() { }

    private Product(ProductId id, string name, string ruName, string slug, ProductType type)
    {
        SetId(id);
        SetName(name);
        SetRuName(ruName);
        SetSlug(slug);
        SetType(type);
    }

    public static Product Create(ProductId id, string name, string ruName, string slug, ProductType type)
        => new(id, name, ruName, slug, type);

    public void Update(string name, string ruName, string slug, ProductType type)
    {
        SetName(name);
        SetRuName(ruName);
        SetSlug(slug);
        SetType(type);
    }

    public void AddCategory(ProductCategoryId categoryId)
    {
        EnsureCategoryNotAttached(categoryId);
        _categories.Add(ProductCategoryReference.Create(categoryId));
    }

    public void RemoveCategory(ProductCategoryId categoryId)
    {
        var category = GetCategory(categoryId);
        _categories.Remove(category);
    }

    public void ReplaceCategories(IEnumerable<ProductCategoryId> categoryIds)
    {
        ArgumentNullException.ThrowIfNull(categoryIds);

        var uniqueCategoryIds = categoryIds.Distinct().ToList();

        _categories.Clear();
        foreach (var categoryId in uniqueCategoryIds)
            _categories.Add(ProductCategoryReference.Create(categoryId));
    }

    public void AddPhoto(
        ProductPhotoId photoId,
        MediaFileId mediaFileId,
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

        _photos.Add(ProductPhoto.Create(photoId, mediaFileId, alt, isVisible, sortOrder, shouldBeMain));
    }

    public void AddPhoto(
        ProductPhotoId photoId,
        MediaFile mediaFile,
        string? alt = null,
        bool isVisible = true,
        int sortOrder = 0,
        bool isMain = false)
    {
        EnsureMediaFileReady(mediaFile);

        AddPhoto(photoId, mediaFile.Id, alt, isVisible, sortOrder, isMain);
    }

    public void SetPhotoVisibility(ProductPhotoId photoId, bool isVisible)
    {
        var photo = GetPhoto(photoId);
        photo.SetVisibility(isVisible);
    }

    public void SetPhotoAltText(ProductPhotoId photoId, string? alt)
    {
        var photo = GetPhoto(photoId);
        photo.SetAltText(alt);
    }

    public void ReplacePhotoMediaFile(ProductPhotoId photoId, MediaFileId mediaFileId)
    {
        var photo = GetPhoto(photoId);
        EnsureMediaFileNotAttached(mediaFileId, photoId);
        photo.ReplaceMediaFile(mediaFileId);
    }

    public void ReplacePhotoMediaFile(ProductPhotoId photoId, MediaFile mediaFile)
    {
        EnsureMediaFileReady(mediaFile);

        ReplacePhotoMediaFile(photoId, mediaFile.Id);
    }

    public void SetPhotoSortOrder(ProductPhotoId photoId, int sortOrder)
    {
        var photo = GetPhoto(photoId);
        photo.SetSortOrder(sortOrder);
    }

    public void SetMainPhoto(ProductPhotoId photoId)
    {
        var photo = GetPhoto(photoId);

        ClearMainPhoto();
        photo.SetMain(true);
    }

    public void RemovePhoto(ProductPhotoId photoId)
    {
        var photo = GetPhoto(photoId);
        var removedMainPhoto = photo.IsMain;
        _photos.Remove(photo);

        if (removedMainPhoto)
            EnsureMainPhotoSelected();
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