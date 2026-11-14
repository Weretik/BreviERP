using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;

namespace Reference.Domain.FabricColors;

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
    public static DomainResult<FabricColor> Create(FabricColorId id, FabricColorDetails details)
    {
        if (id.Value == default) throw new ArgumentException("Fabric colour id is required.", nameof(id));
        var color = new FabricColor { Id = id };
        var applied = color.ApplyDetails(details);
        return applied.IsFailure ? applied.Error! : color;
    }

    public DomainResult ApplyDetails(FabricColorDetails details)
    {
        if (string.IsNullOrWhiteSpace(details.Name)) return FabricColorErrors.NameIsRequired();
        var name = details.Name.Trim();
        if (name.Length > NameMaxLength) return FabricColorErrors.NameIsTooLong(NameMaxLength);

        Name = name;
        IsActive = details.IsActive;
        SortOrder = details.SortOrder;
        return DomainResult.Success();
    }
    #endregion
}
