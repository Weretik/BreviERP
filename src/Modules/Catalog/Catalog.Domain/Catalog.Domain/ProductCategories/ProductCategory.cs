using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;

namespace Catalog.Domain.ProductCategories;

public class ProductCategory : BaseEntity<ProductCategoryId>, IAggregateRoot
{
    private const int NameMaxLength = 200;
    private const int SlugMaxLength = 160;
    private const int DescriptionMaxLength = 1000;
    private const int PathMaxLength = 900;
    private static readonly Regex SlugRegex = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public string Name { get; private set; } = null!;
    public string RuName { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public ProductCategoryId? ParentId { get; private set; }
    public string Path { get; private set; } = null!;
    public int Level { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private ProductCategory() { }

    public static DomainResult<ProductCategory> Create(
        ProductCategoryId id,
        string name,
        string ruName,
        string slug,
        ProductCategoryId? parentId = null,
        string? parentPath = null,
        int sortOrder = 0,
        bool isActive = true,
        string? description = null)
    {
        if (id.Value == default) throw new ArgumentException("A category needs an id.", nameof(id));
        var category = new ProductCategory { Id = id };
        var applied = category.Apply(name, ruName, slug, sortOrder, isActive, description);
        if (applied.IsFailure) return applied.Error!;
        var placed = category.SetParent(parentId, parentPath);
        if (placed.IsFailure) return placed.Error!;
        return category;
    }

    public DomainResult Update(
        string name,
        string ruName,
        string slug,
        int sortOrder,
        bool isActive,
        string? description = null) =>
        Apply(name, ruName, slug, sortOrder, isActive, description);

    public DomainResult MoveTo(ProductCategoryId? parentId, string? parentPath) => SetParent(parentId, parentPath);

    public DomainResult RebasePath(string oldAncestorPath, string newAncestorPath)
    {
        var oldPath = NormalizeParentPath(oldAncestorPath);
        if (oldPath.IsFailure) return oldPath.Error!;
        var newPath = NormalizeParentPath(newAncestorPath);
        if (newPath.IsFailure) return newPath.Error!;
        if (!Path.StartsWith(oldPath.Value, StringComparison.Ordinal))
            return ProductCategoryErrors.PathAncestorMismatch();
        var path = $"{newPath.Value}{Path[oldPath.Value.Length..]}";
        if (path.Length > PathMaxLength)
            return ProductCategoryErrors.PathLengthInvalid(PathMaxLength);
        Path = path;
        Level = Path.Count(x => x == '/') - 2;
        return DomainResult.Success();
    }

    /// <summary>Checks every field first, so a refused change leaves the category as it was.</summary>
    private DomainResult Apply(string name, string ruName, string slug, int sortOrder, bool isActive, string? description)
    {
        var cleanName = CleanName(name);
        if (cleanName.IsFailure) return cleanName.Error!;
        var cleanRuName = CleanName(ruName);
        if (cleanRuName.IsFailure) return cleanRuName.Error!;
        var cleanSlug = CleanSlug(slug);
        if (cleanSlug.IsFailure) return cleanSlug.Error!;
        if (sortOrder < 0) return ProductCategoryErrors.SortOrderOutOfRange();
        Name = cleanName.Value;
        RuName = cleanRuName.Value;
        Slug = cleanSlug.Value;
        SortOrder = sortOrder;
        Description = CleanDescription(description);
        IsActive = isActive;
        return DomainResult.Success();
    }

    private static DomainResult<string> CleanName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ProductCategoryErrors.NameIsRequired();
        var normalizedName = name.Trim();
        if (normalizedName.Length > NameMaxLength)
            return ProductCategoryErrors.NameLengthInvalid(NameMaxLength);
        return normalizedName;
    }

    private static DomainResult<string> CleanSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return ProductCategoryErrors.SlugIsRequired();
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        if (normalizedSlug.Length > SlugMaxLength || !SlugRegex.IsMatch(normalizedSlug))
            return ProductCategoryErrors.SlugFormatInvalid();
        return normalizedSlug;
    }

    private static string? CleanDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;
        var normalizedDescription = description.Trim();
        return normalizedDescription.Length > DescriptionMaxLength
            ? normalizedDescription[..DescriptionMaxLength]
            : normalizedDescription;
    }

    private DomainResult SetParent(ProductCategoryId? parentId, string? parentPath)
    {
        if (parentId == Id)
            return ProductCategoryErrors.ParentCannotBeSelf();
        var normalizedParentPath = NormalizeParentPath(parentPath);
        if (normalizedParentPath.IsFailure) return normalizedParentPath.Error!;
        var ownPathSegment = $"/{Id.Value}/";
        if (normalizedParentPath.Value.Contains(ownPathSegment, StringComparison.Ordinal))
            return ProductCategoryErrors.ParentCannotBeDescendant();
        var path = $"{normalizedParentPath.Value}{Id.Value}/";
        if (path.Length > PathMaxLength)
            return ProductCategoryErrors.PathLengthInvalid(PathMaxLength);
        ParentId = parentId;
        Path = path;
        Level = Path.Count(x => x == '/') - 2;
        return DomainResult.Success();
    }

    private static DomainResult<string> NormalizeParentPath(string? parentPath)
    {
        if (string.IsNullOrWhiteSpace(parentPath))
            return "/";
        var normalizedPath = parentPath.Trim();
        if (normalizedPath.Length > PathMaxLength)
            return ProductCategoryErrors.PathLengthInvalid(PathMaxLength);
        if (!normalizedPath.StartsWith('/'))
            normalizedPath = $"/{normalizedPath}";
        if (!normalizedPath.EndsWith('/'))
            normalizedPath = $"{normalizedPath}/";
        return normalizedPath;
    }
}
