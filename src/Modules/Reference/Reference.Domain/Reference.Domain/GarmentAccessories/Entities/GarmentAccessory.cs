using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.GarmentAccessories.Errors;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Domain.GarmentAccessories.Entities;

/// <summary>
/// An accessory from the Calculator sheet «Фурнітура». <see cref="Code"/> (FUR-001…) is the stable key for the
/// sheet import; <see cref="Unit"/> is one of <see cref="Units"/>.
/// </summary>
public class GarmentAccessory : BaseEntity<GarmentAccessoryId>, IAggregateRoot
{
    public const int CodeMaxLength = 20;
    public const int NameMaxLength = 500;
    public const int CommentMaxLength = 1000;
    public const string DefaultUnit = "шт.";
    public static readonly IReadOnlyList<string> Units = ["шт.", "м", "послуга"];

    #region Properties
    public string? Code { get; private set; }
    public string Name { get; private set; } = null!;
    public MoneyAmount Price { get; private set; }
    public DateOnly? PriceUpdatedAt { get; private set; }
    public SupplierId SupplierId { get; private set; }
    public string Unit { get; private set; } = DefaultUnit;
    public bool IsActive { get; private set; } = true;
    public string? Comment { get; private set; }
    #endregion

    #region Constructors
    private GarmentAccessory() { }

    private GarmentAccessory(GarmentAccessoryId id, string name, MoneyAmount price, int supplierId = 1)
    {
        SetId(id);
        SetName(name);
        SetPrice(price);
        SetSupplierId(supplierId);
    }
    #endregion

    #region Factories
    public static GarmentAccessory Create(
        GarmentAccessoryId id,
        string name,
        MoneyAmount price,
        int supplierId = 1)
        => new(id, name, price, supplierId);

    public static GarmentAccessory Create(
        GarmentAccessoryId id,
        GarmentAccessoryDetails details,
        MoneyAmount price,
        DateOnly? priceUpdatedAt)
    {
        var accessory = new GarmentAccessory(id, details.Name, price, details.SupplierId.Value);
        accessory.ApplyDetails(details);
        accessory.PriceUpdatedAt = priceUpdatedAt;
        return accessory;
    }

    public void Update(string name, MoneyAmount price, int supplierId = 1)
    {
        SetName(name);
        SetPrice(price);
        SetSupplierId(supplierId);
    }

    /// <summary>Replaces every card field except price.</summary>
    public void ApplyDetails(GarmentAccessoryDetails details)
    {
        Code = NormalizeCode(details.Code);
        SetName(details.Name);
        SetSupplierId(details.SupplierId.Value);
        Unit = NormalizeUnit(details.Unit)
            ?? throw new DomainException(GarmentAccessoryErrors.UnitInvalid(string.Join(", ", Units)));
        IsActive = details.IsActive;
        Comment = Text(details.Comment, CommentMaxLength);
    }

    /// <summary>Sets the price and the date it was confirmed (the sheet's «Дата оновлення ціни»).</summary>
    public void SetPrice(MoneyAmount price, DateOnly? priceUpdatedAt)
    {
        SetPrice(price);
        PriceUpdatedAt = priceUpdatedAt;
    }

    public static string? NormalizeCode(string? code) =>
        Text(code, CodeMaxLength)?.ToUpperInvariant();

    /// <summary>Maps sheet spellings («шт», «м.», «Послуга») to a known unit, or null when unknown.</summary>
    public static string? NormalizeUnit(string? unit)
    {
        var key = unit?.Trim().TrimEnd('.').ToLowerInvariant();
        return Units.FirstOrDefault(known => known.TrimEnd('.') == key);
    }
    #endregion

    #region Setters/Validation
    private static string? Text(string? value, int maxLength) =>
        ReferenceText.Optional(value, maxLength, max => new DomainException(GarmentAccessoryErrors.TextTooLong(max)));

    private void SetId(GarmentAccessoryId id)
    {
        if (id.Value == default)
            throw new DomainException(GarmentAccessoryErrors.IdIsRequired());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(GarmentAccessoryErrors.NameIsRequired());

        Name = Text(name, NameMaxLength)!;
    }

    private void SetPrice(MoneyAmount price)
    {
        if (price.Value < 0 || price.Value > 10_000)
            throw new DomainException(GarmentAccessoryErrors.PriceOutOfRange(0, 10_000));

        Price = price;
    }

    private void SetSupplierId(int supplierId)
    {
        if (supplierId <= 0)
            throw new DomainException(GarmentAccessoryErrors.SupplierIdIsRequired());

        SupplierId = Reference.Domain.Suppliers.ValueObjects.SupplierId.From(supplierId);
    }
    #endregion
}
