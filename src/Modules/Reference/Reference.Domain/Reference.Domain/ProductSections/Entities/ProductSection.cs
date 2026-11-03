using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
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
    public static ProductSection Create(ProductSectionId id, ProductSectionDetails details)
    {
        if (id.Value == default)
            throw new DomainException(ProductSectionErrors.IdIsRequired());

        var section = new ProductSection { Id = id };
        section.ApplyDetails(details);
        return section;
    }

    public void ApplyDetails(ProductSectionDetails details)
    {
        if (!Enum.IsDefined(details.Type))
            throw new DomainException(ProductSectionErrors.TypeIsInvalid());
        if (string.IsNullOrWhiteSpace(details.Name))
            throw new DomainException(ProductSectionErrors.NameIsRequired());

        Type = details.Type;
        Name = Text(details.Name, NameMaxLength, "Назва розділу")!;
        IsActive = details.IsActive;
        Comment = Text(details.Comment, CommentMaxLength, "Коментар");
        SortOrder = details.SortOrder;
    }
    #endregion

    #region Validation
    private static string? Text(string? text, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        var trimmed = text.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException(ProductSectionErrors.TextIsTooLong(field, maxLength));
        return trimmed;
    }
    #endregion
}
