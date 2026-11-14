using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;
using Reference.Domain.ProductSections.Enums;
using Reference.Domain.ProductSections.Errors;
using Reference.Domain.ProductSections.ValueObjects;

namespace Reference.Domain.ProductSections.Entities;

/// <summary>
/// A product section («Розділ») of the Calculator sheet «Типи і розділи»: «Одяг ЛІТО», «Захист рук»… Each section
/// belongs to one product type. Products refer to a section by ID, so its name can be corrected later; the public
/// site builds its categories from these sections.
/// </summary>
public class ProductSection : BaseEntity<ProductSectionId>, IAggregateRoot
{
    public const int NameMaxLength = 200;
    public const int CommentMaxLength = 1000;

    #region Properties
    public ProductSectionType Type { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public string? Comment { get; private set; }
    public int SortOrder { get; private set; }
    #endregion

    #region Constructors
    private ProductSection() { }
    #endregion

    #region Factories
    public static DomainResult<ProductSection> Create(ProductSectionId id, ProductSectionDetails details)
    {
        if (id.Value == default) throw new ArgumentException("Product section id is required.", nameof(id));
        var section = new ProductSection { Id = id };
        var applied = section.ApplyDetails(details);
        return applied.IsFailure ? applied.Error! : section;
    }

    public DomainResult ApplyDetails(ProductSectionDetails details)
    {
        var check = DomainResult.Combine(
            Enum.IsDefined(details.Type) ? DomainResult.Success() : ProductSectionErrors.TypeIsInvalid(),
            string.IsNullOrWhiteSpace(details.Name)
                ? ProductSectionErrors.NameIsRequired()
                : Text(details.Name, NameMaxLength, "Назва розділу"),
            Text(details.Comment, CommentMaxLength, "Коментар"));
        if (check.IsFailure) return check;

        Type = details.Type;
        Name = details.Name.Trim();
        IsActive = details.IsActive;
        Comment = string.IsNullOrWhiteSpace(details.Comment) ? null : details.Comment.Trim();
        SortOrder = details.SortOrder;
        return DomainResult.Success();
    }
    #endregion

    #region Rules
    private static DomainResult Text(string? text, int maxLength, string field) =>
        text?.Trim() is { Length: var length } && length > maxLength
            ? ProductSectionErrors.TextIsTooLong(field, maxLength)
            : DomainResult.Success();
    #endregion
}
