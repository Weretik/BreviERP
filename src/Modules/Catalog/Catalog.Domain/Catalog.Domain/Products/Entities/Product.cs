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
        if (type == ProductType.Ppe && Type == ProductType.Sewing &&
            (_articles.Count != 0 || _materialLines.Count != 0 || _operationLines.Count != 0))
            throw new DomainException(ProductErrors.TypeSpecificDataInvalid());
        SetName(name);
        SetRuName(ruName);
        SetSlug(slug);
        SetType(type);
        ClearDetailsForOtherType();
        MarkAsUpdated(updatedAt);
    }

    public void SetModelCode(string? modelCode, DateTimeOffset updatedAt)
    {
        if (Type != ProductType.Sewing) throw new DomainException(ProductErrors.TypeSpecificDataInvalid());
        var normalized = string.IsNullOrWhiteSpace(modelCode) ? null : modelCode.Trim();
        if (normalized?.Length > 80) throw new DomainException(ProductErrors.ModelCodeInvalid());
        ModelCode = normalized;
        CompositionVersion++;
        MarkAsUpdated(updatedAt);
    }

    public void AddArticle(SewingArticle article, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(article);
        if (Type != ProductType.Sewing || article.ProductId != Id) throw new DomainException(ProductErrors.DetailsBelongToAnotherProduct());
        if (_articles.Count >= 200) throw new DomainException(ProductErrors.ArticleLimitReached());
        if (_articles.Any(x => string.Equals(x.SaleSku, article.SaleSku, StringComparison.Ordinal)))
            throw new DomainException(ProductErrors.DuplicateSaleSku(article.SaleSku));
        _articles.Add(article);
        CompositionVersion++;
        MarkAsUpdated(updatedAt);
    }

    public void ReplaceComposition(IEnumerable<SewingMaterialLine> materialLines,
        IEnumerable<SewingOperationLine> operationLines, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(materialLines);
        ArgumentNullException.ThrowIfNull(operationLines);
        if (Type != ProductType.Sewing) throw new DomainException(ProductErrors.TypeSpecificDataInvalid());
        var materials = materialLines.ToList();
        var operations = operationLines.ToList();
        if (materials.Any(x => x.ProductId != Id || (x.ArticleId.HasValue && !_articles.Any(a => a.Id == x.ArticleId))))
            throw new DomainException(ProductErrors.CompositionParentMismatch());
        if (operations.Any(x => x.ProductId != Id || (x.ArticleId.HasValue && !_articles.Any(a => a.Id == x.ArticleId))))
            throw new DomainException(ProductErrors.CompositionParentMismatch());
        var commonKeys = materials.Where(x => !x.ArticleId.HasValue).Select(x => (x.Kind, x.ReferenceKey)).ToHashSet();
        if (materials.Where(x => x.ArticleId.HasValue).Any(x => commonKeys.Contains((x.Kind, x.ReferenceKey))))
            throw new DomainException(ProductErrors.CompositionScopeConflict());
        if (materials.GroupBy(x => (x.ArticleId, x.Kind, x.ReferenceKey)).Any(g => g.Count() > 1) ||
            operations.GroupBy(x => x.OperationId).Any(g => g.Count() > 1))
            throw new DomainException(ProductErrors.DuplicateCompositionLine());
        if (materials.Count > 2_000 || operations.Count > 2_000)
            throw new DomainException(ProductErrors.CompositionLineLimitReached());
        _materialLines.Clear(); _materialLines.AddRange(materials);
        _operationLines.Clear(); _operationLines.AddRange(operations);
        CompositionVersion++;
        MarkAsUpdated(updatedAt);
    }

    public IReadOnlyList<string> GetCalculationReadinessBlockers(ArticleId? articleId = null)
    {
        if (articleId.HasValue && !_articles.Any(a => a.Id == articleId.Value))
            return ["Article does not belong to this product."];
        if (articleId.HasValue && !_articles.Single(a => a.Id == articleId.Value).IsActive)
            return ["Article is inactive."];
        var materials = _materialLines.Where(x => !x.ArticleId.HasValue || x.ArticleId == articleId);
        var blockers = materials.Where(x => x.IsRequired && !x.IsReviewed)
            .Select(x => $"Required material line {x.Id.Value} has not been reviewed.").ToList();
        blockers.AddRange(_operationLines.Where(x => x.IsRequired && !x.IsReviewed)
            .Select(x => $"Required operation line {x.Id.Value} has not been reviewed."));
        blockers.AddRange(materials.Where(x => x.Kind == SewingMaterialKind.OtherCost)
            .Select(x => $"Other cost line {x.Id.Value} has no costing reference owner."));
        if (!materials.Any(x => x.Kind == SewingMaterialKind.Fabric)) blockers.Add("No effective fabric line is configured.");
        if (_operationLines.Count == 0) blockers.Add("No operation lines are configured.");
        return blockers;
    }

    public IReadOnlyList<SewingMaterialLine> GetEffectiveMaterials(ArticleId? articleId = null)
    {
        if (articleId.HasValue && !_articles.Any(a => a.Id == articleId.Value))
            throw new DomainException(ProductErrors.CompositionParentMismatch());
        return _materialLines.Where(x => !x.ArticleId.HasValue || x.ArticleId == articleId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value).ToArray();
    }

    public void ReplaceArticleComposition(ArticleId articleId, IEnumerable<SewingMaterialLine> materialLines,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(materialLines);
        var articleLines = materialLines.ToList();
        if (!_articles.Any(a => a.Id == articleId) || articleLines.Any(x => x.ProductId != Id || x.ArticleId != articleId))
            throw new DomainException(ProductErrors.CompositionParentMismatch());
        var candidate = _materialLines.Where(x => x.ArticleId != articleId).Concat(articleLines).ToList();
        ReplaceComposition(candidate, _operationLines, updatedAt);
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
