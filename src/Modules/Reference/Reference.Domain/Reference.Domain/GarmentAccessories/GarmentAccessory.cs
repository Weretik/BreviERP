using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;
using Reference.Domain.Suppliers;

namespace Reference.Domain.GarmentAccessories;

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
    #endregion

    #region Factories
    public static DomainResult<GarmentAccessory> Create(
        GarmentAccessoryId id,
        string name,
        MoneyAmount price,
        int supplierId = 1)
    {
        RequireId(id);
        var check = DomainResult.Combine(CheckName(name), CheckPrice(price), CheckSupplier(supplierId));
        if (check.IsFailure) return check.Error!;
        var accessory = new GarmentAccessory { Id = id };
        accessory.SetMain(name, price, supplierId);
        return accessory;
    }

    public static DomainResult<GarmentAccessory> Create(
        GarmentAccessoryId id,
        GarmentAccessoryDetails details,
        MoneyAmount price,
        DateOnly? priceUpdatedAt)
    {
        RequireId(id);
        var check = DomainResult.Combine(CheckDetails(details), CheckPrice(price));
        if (check.IsFailure) return check.Error!;
        var accessory = new GarmentAccessory { Id = id };
        accessory.SetMain(details.Name, price, details.SupplierId.Value);
        accessory.SetDetails(details);
        accessory.PriceUpdatedAt = priceUpdatedAt;
        return accessory;
    }

    public DomainResult Update(string name, MoneyAmount price, int supplierId = 1)
    {
        var check = DomainResult.Combine(CheckName(name), CheckPrice(price), CheckSupplier(supplierId));
        if (check.IsFailure) return check;
        SetMain(name, price, supplierId);
        return DomainResult.Success();
    }

    /// <summary>Replaces every card field except price.</summary>
    public DomainResult ApplyDetails(GarmentAccessoryDetails details)
    {
        var check = CheckDetails(details);
        if (check.IsFailure) return check;
        Name = ReferenceText.Optional(details.Name)!;
        SupplierId = details.SupplierId;
        SetDetails(details);
        return DomainResult.Success();
    }

    /// <summary>Sets the price and the date it was confirmed (the sheet's «Дата оновлення ціни»).</summary>
    public DomainResult SetPrice(MoneyAmount price, DateOnly? priceUpdatedAt)
    {
        var check = CheckPrice(price);
        if (check.IsFailure) return check;
        Price = price;
        PriceUpdatedAt = priceUpdatedAt;
        return DomainResult.Success();
    }

    /// <summary>The upper-case code, or null for none; check it first with <see cref="CheckCode"/>.</summary>
    public static string? NormalizeCode(string? code) => ReferenceText.Optional(code)?.ToUpperInvariant();

    public static DomainResult CheckCode(string? code) => Text(code, CodeMaxLength);

    /// <summary>Maps sheet spellings («шт», «м.», «Послуга») to a known unit, or null when unknown.</summary>
    public static string? NormalizeUnit(string? unit)
    {
        var key = unit?.Trim().TrimEnd('.').ToLowerInvariant();
        return Units.FirstOrDefault(known => known.TrimEnd('.') == key);
    }
    #endregion

    #region Rules
    public static DomainResult CheckDetails(GarmentAccessoryDetails details) => DomainResult.Combine(
        CheckCode(details.Code),
        CheckName(details.Name),
        CheckSupplier(details.SupplierId.Value),
        NormalizeUnit(details.Unit) is null
            ? GarmentAccessoryErrors.UnitInvalid(string.Join(", ", Units))
            : DomainResult.Success(),
        Text(details.Comment, CommentMaxLength));

    private static DomainResult CheckName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? GarmentAccessoryErrors.NameIsRequired() : Text(name, NameMaxLength);

    private static DomainResult CheckPrice(MoneyAmount price) =>
        price.Value is < 0 or > 10_000 ? GarmentAccessoryErrors.PriceOutOfRange(0, 10_000) : DomainResult.Success();

    private static DomainResult CheckSupplier(int supplierId) =>
        supplierId <= 0 ? GarmentAccessoryErrors.SupplierIdIsRequired() : DomainResult.Success();

    private static DomainResult Text(string? value, int maxLength) =>
        ReferenceText.Check(value, maxLength, GarmentAccessoryErrors.TextTooLong);

    private static void RequireId(GarmentAccessoryId id)
    {
        if (id.Value == default) throw new ArgumentException("Accessory id is required.", nameof(id));
    }

    private void SetMain(string name, MoneyAmount price, int supplierId)
    {
        Name = ReferenceText.Optional(name)!;
        Price = price;
        SupplierId = Reference.Domain.Suppliers.SupplierId.From(supplierId);
    }

    private void SetDetails(GarmentAccessoryDetails details)
    {
        Code = NormalizeCode(details.Code);
        Unit = NormalizeUnit(details.Unit)!;
        IsActive = details.IsActive;
        Comment = ReferenceText.Optional(details.Comment);
    }
    #endregion
}
