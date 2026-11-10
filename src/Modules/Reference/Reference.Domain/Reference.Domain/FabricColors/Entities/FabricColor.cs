using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.FabricColors.Errors;
using Reference.Domain.FabricColors.ValueObjects;

namespace Reference.Domain.FabricColors.Entities;

/// <summary>
/// A colour of «Кольори» (sheet «Тканини», column «Загальний перелік кольорів»): «Чорний», «Темно-синій кол.1076»…
/// The workshop picks colours from this list for an item, a fabric receipt and the stock, where the stock line and its
/// reservations are keyed by fabric and colour. Records keep the colour as text, so a rename does not change them.
/// </summary>
public class FabricColor : BaseEntity<FabricColorId>, IAggregateRoot
{
    /// <summary>The same limit as a colour of a stock line in «Склад тканин».</summary>
    public const int NameMaxLength = 80;

    #region Properties
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    #endregion

    #region Constructors
    private FabricColor() { }
    #endregion

    #region Factories
    public static FabricColor Create(FabricColorId id, FabricColorDetails details)
    {
        if (id.Value == default)
            throw new DomainException(FabricColorErrors.IdIsRequired());

        var color = new FabricColor { Id = id };
        color.ApplyDetails(details);
        return color;
    }

    public void ApplyDetails(FabricColorDetails details)
    {
        if (string.IsNullOrWhiteSpace(details.Name))
            throw new DomainException(FabricColorErrors.NameIsRequired());

        var name = details.Name.Trim();
        if (name.Length > NameMaxLength)
            throw new DomainException(FabricColorErrors.NameIsTooLong(NameMaxLength));

        Name = name;
        IsActive = details.IsActive;
        SortOrder = details.SortOrder;
    }
    #endregion
}
