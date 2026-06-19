using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.Errors;
using Reference.Domain.ValueObjects;

namespace Reference.Domain.Entities;

public class ProductCategory : BaseEntity<ProductCategoryId>, IAggregateRoot
{
    private const int NameMaxLength = 200;
    private const int SlugMaxLength = 160;
    private const int DescriptionMaxLength = 1000;
    private const int PathMaxLength = 900;
    private static readonly Regex SlugRegex = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    #region Properties
    public string Name { get; private set; } = null!;
    public string RuName { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public ProductCategoryId? ParentId { get; private set; }
    public string Path { get; private set; } = null!;
    public int Level { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }
    #endregion

    #region Constructors
    private ProductCategory() { }

    private ProductCategory(
        ProductCategoryId id,
        string name,
        string ruName,
        string slug,
        ProductCategoryId? parentId,
        string? parentPath,
        int sortOrder,
        bool isActive,
        string? description)
    {
        SetId(id);
        SetName(name);
        SetRuName(ruName);
        SetSlug(slug);
        SetDescription(description);
        SetParent(parentId, parentPath);
        SetSortOrder(sortOrder);
        IsActive = isActive;
    }
    #endregion

    #region Factories
    public static ProductCategory Create(
        ProductCategoryId id,
        string name,
        string ruName,
        string slug,
        ProductCategoryId? parentId = null,
        string? parentPath = null,
        int sortOrder = 0,
        bool isActive = true,
        string? description = null)
        => new(id, name, ruName, slug, parentId, parentPath, sortOrder, isActive, description);

    public void Update(
        string name,
        string ruName,
        string slug,
        int sortOrder,
        bool isActive,
        string? description = null)
    {
        SetName(name);
        SetRuName(ruName);
        SetSlug(slug);
        SetSortOrder(sortOrder);
        SetDescription(description);
        IsActive = isActive;
    }

    public void MoveTo(ProductCategoryId? parentId, string? parentPath)
    {
        SetParent(parentId, parentPath);
    }
    #endregion

    #region Setters/Validation
    private void SetId(ProductCategoryId id)
    {
        if (id.Value == default)
            throw new DomainException(ProductCategoryErrors.IdIsRequired());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProductCategoryErrors.NameIsRequired());

        var normalizedName = name.Trim();
        if (normalizedName.Length > NameMaxLength)
            throw new DomainException(ProductCategoryErrors.NameLengthInvalid(NameMaxLength));

        Name = normalizedName;
    }

    private void SetRuName(string ruName)
    {
        if (string.IsNullOrWhiteSpace(ruName))
            throw new DomainException(ProductCategoryErrors.NameIsRequired());

        var normalizedRuName = ruName.Trim();
        if (normalizedRuName.Length > NameMaxLength)
            throw new DomainException(ProductCategoryErrors.NameLengthInvalid(NameMaxLength));

        RuName = normalizedRuName;
    }

    private void SetSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException(ProductCategoryErrors.SlugIsRequired());

        var normalizedSlug = slug.Trim().ToLowerInvariant();
        if (normalizedSlug.Length > SlugMaxLength || !SlugRegex.IsMatch(normalizedSlug))
            throw new DomainException(ProductCategoryErrors.SlugFormatInvalid());

        Slug = normalizedSlug;
    }

    private void SetDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Description = null;
            return;
        }

        var normalizedDescription = description.Trim();
        Description = normalizedDescription.Length > DescriptionMaxLength
            ? normalizedDescription[..DescriptionMaxLength]
            : normalizedDescription;
    }

    private void SetParent(ProductCategoryId? parentId, string? parentPath)
    {
        if (parentId == Id)
            throw new DomainException(ProductCategoryErrors.ParentCannotBeSelf());

        var normalizedParentPath = NormalizeParentPath(parentPath);
        var ownPathSegment = $"/{Id.Value}/";
        if (normalizedParentPath.Contains(ownPathSegment, StringComparison.Ordinal))
            throw new DomainException(ProductCategoryErrors.ParentCannotBeDescendant());

        ParentId = parentId;
        Path = $"{normalizedParentPath}{Id.Value}/";

        if (Path.Length > PathMaxLength)
            throw new DomainException(ProductCategoryErrors.PathLengthInvalid(PathMaxLength));

        Level = Path.Count(x => x == '/') - 2;
    }

    private void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
            throw new DomainException(ProductCategoryErrors.SortOrderOutOfRange());

        SortOrder = sortOrder;
    }

    private static string NormalizeParentPath(string? parentPath)
    {
        if (string.IsNullOrWhiteSpace(parentPath))
            return "/";

        var normalizedPath = parentPath.Trim();
        if (normalizedPath.Length > PathMaxLength)
            throw new DomainException(ProductCategoryErrors.PathLengthInvalid(PathMaxLength));

        if (!normalizedPath.StartsWith('/'))
            normalizedPath = $"/{normalizedPath}";

        if (!normalizedPath.EndsWith('/'))
            normalizedPath = $"{normalizedPath}/";

        return normalizedPath;
    }
    #endregion
}
