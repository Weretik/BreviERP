using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;
using Reference.Domain.GarmentAccessories.Errors;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;

namespace Reference.Domain.GarmentAccessories.Entities;

/// <summary>
/// A fabric from the Calculator sheet «Тканини». <see cref="Code"/> (FAB-001…) is the stable key for the sheet
/// import; older Brevi rows may have none until the import links them.
/// </summary>
public class Fabric : BaseEntity<FabricId>, IAggregateRoot
{
    public const int CodeMaxLength = 20;
    public const int NameMaxLength = 500;
    public const int CompositionMaxLength = 500;
    public const int DensityMaxLength = 100;
    public const int CommentMaxLength = 1000;
    public const int ProposalDescriptionMaxLength = 1000;

    #region Properties
    public string? Code { get; private set; }
    public string Name { get; private set; } = null!;
    public string? NameRu { get; private set; }
    public MoneyAmount Price { get; private set; }
    public DateOnly? PriceUpdatedAt { get; private set; }
    public SupplierId ProviderId { get; private set; }
    public string? Composition { get; private set; }
    public string? CompositionRu { get; private set; }
    public string? Density { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Comment { get; private set; }
    public string? ProposalDescription { get; private set; }
    #endregion

    #region Constructors
    private Fabric() { }
    #endregion

    #region Factories
    public static DomainResult<Fabric> Create(FabricId id, string name, MoneyAmount price, SupplierId providerId)
    {
        RequireId(id);
        var check = DomainResult.Combine(CheckName(name), CheckPrice(price), CheckProvider(providerId));
        if (check.IsFailure) return check.Error!;
        var fabric = new Fabric { Id = id };
        fabric.SetMain(name, price, providerId);
        return fabric;
    }

    public static DomainResult<Fabric> Create(FabricId id, FabricDetails details, MoneyAmount price,
        DateOnly? priceUpdatedAt)
    {
        RequireId(id);
        var check = DomainResult.Combine(CheckDetails(details), CheckPrice(price));
        if (check.IsFailure) return check.Error!;
        var fabric = new Fabric { Id = id };
        fabric.SetMain(details.Name, price, details.ProviderId);
        fabric.SetDetails(details);
        fabric.PriceUpdatedAt = priceUpdatedAt;
        return fabric;
    }

    public DomainResult Update(string name, MoneyAmount price, SupplierId providerId)
    {
        var check = DomainResult.Combine(CheckName(name), CheckPrice(price), CheckProvider(providerId));
        if (check.IsFailure) return check;
        SetMain(name, price, providerId);
        return DomainResult.Success();
    }

    /// <summary>Replaces every card field except price.</summary>
    public DomainResult ApplyDetails(FabricDetails details)
    {
        var check = CheckDetails(details);
        if (check.IsFailure) return check;
        Name = ReferenceText.Optional(details.Name)!;
        ProviderId = details.ProviderId;
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

    public DomainResult SetPresentationMetadata(string? composition, string? density)
    {
        var check = DomainResult.Combine(Text(composition, CompositionMaxLength), Text(density, DensityMaxLength));
        if (check.IsFailure) return check;
        Composition = ReferenceText.Optional(composition);
        Density = ReferenceText.Optional(density);
        return DomainResult.Success();
    }

    /// <summary>The upper-case code, or null for none; check it first with <see cref="CheckCode"/>.</summary>
    public static string? NormalizeCode(string? code) => ReferenceText.Optional(code)?.ToUpperInvariant();

    public static DomainResult CheckCode(string? code) => Text(code, CodeMaxLength);
    #endregion

    #region Rules
    public static DomainResult CheckDetails(FabricDetails details) => DomainResult.Combine(
        CheckCode(details.Code),
        CheckName(details.Name),
        Text(details.NameRu, NameMaxLength),
        CheckProvider(details.ProviderId),
        Text(details.Composition, CompositionMaxLength),
        Text(details.CompositionRu, CompositionMaxLength),
        Text(details.Density, DensityMaxLength),
        Text(details.Comment, CommentMaxLength),
        Text(details.ProposalDescription, ProposalDescriptionMaxLength));

    private static DomainResult CheckName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? FabricErrors.NameIsRequired() : Text(name, NameMaxLength);

    private static DomainResult CheckPrice(MoneyAmount price) =>
        price.Value is < 0 or > 10_000 ? FabricErrors.PriceOutOfRange(0, 10_000) : DomainResult.Success();

    private static DomainResult CheckProvider(SupplierId providerId) =>
        providerId.Value == default ? FabricErrors.ProviderIdIsRequired() : DomainResult.Success();

    private static DomainResult Text(string? value, int maxLength) =>
        ReferenceText.Check(value, maxLength, FabricErrors.TextTooLong);

    private static void RequireId(FabricId id)
    {
        if (id.Value == default) throw new ArgumentException("Fabric id is required.", nameof(id));
    }

    private void SetMain(string name, MoneyAmount price, SupplierId providerId)
    {
        Name = ReferenceText.Optional(name)!;
        Price = price;
        ProviderId = providerId;
    }

    private void SetDetails(FabricDetails details)
    {
        Code = NormalizeCode(details.Code);
        NameRu = ReferenceText.Optional(details.NameRu);
        Composition = ReferenceText.Optional(details.Composition);
        CompositionRu = ReferenceText.Optional(details.CompositionRu);
        Density = ReferenceText.Optional(details.Density);
        IsActive = details.IsActive;
        Comment = ReferenceText.Optional(details.Comment);
        ProposalDescription = ReferenceText.Optional(details.ProposalDescription);
    }
    #endregion
}
