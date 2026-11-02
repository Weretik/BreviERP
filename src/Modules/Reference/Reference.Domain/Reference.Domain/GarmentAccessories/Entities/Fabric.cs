using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
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

    private Fabric(FabricId id, string name, MoneyAmount price, SupplierId providerId)
    {
        SetId(id);
        SetName(name);
        SetPrice(price);
        SetProviderId(providerId);
    }
    #endregion

    #region Factories
    public static Fabric Create(FabricId id, string name, MoneyAmount price, SupplierId providerId)
        => new(id, name, price, providerId);

    public static Fabric Create(FabricId id, FabricDetails details, MoneyAmount price, DateOnly? priceUpdatedAt)
    {
        var fabric = new Fabric(id, details.Name, price, details.ProviderId);
        fabric.ApplyDetails(details);
        fabric.PriceUpdatedAt = priceUpdatedAt;
        return fabric;
    }

    public void Update(string name, MoneyAmount price, SupplierId providerId)
    {
        SetName(name);
        SetPrice(price);
        SetProviderId(providerId);
    }

    /// <summary>Replaces every card field except price.</summary>
    public void ApplyDetails(FabricDetails details)
    {
        Code = NormalizeCode(details.Code);
        SetName(details.Name);
        NameRu = Text(details.NameRu, NameMaxLength);
        SetProviderId(details.ProviderId);
        Composition = Text(details.Composition, CompositionMaxLength);
        CompositionRu = Text(details.CompositionRu, CompositionMaxLength);
        Density = Text(details.Density, DensityMaxLength);
        IsActive = details.IsActive;
        Comment = Text(details.Comment, CommentMaxLength);
        ProposalDescription = Text(details.ProposalDescription, ProposalDescriptionMaxLength);
    }

    /// <summary>Sets the price and the date it was confirmed (the sheet's «Дата оновлення ціни»).</summary>
    public void SetPrice(MoneyAmount price, DateOnly? priceUpdatedAt)
    {
        SetPrice(price);
        PriceUpdatedAt = priceUpdatedAt;
    }

    public void SetPresentationMetadata(string? composition, string? density)
    {
        Composition = Text(composition, CompositionMaxLength);
        Density = Text(density, DensityMaxLength);
    }

    public static string? NormalizeCode(string? code) =>
        Text(code, CodeMaxLength)?.ToUpperInvariant();
    #endregion

    #region Setters/Validation
    private static string? Text(string? value, int maxLength) =>
        ReferenceText.Optional(value, maxLength, max => new DomainException(FabricErrors.TextTooLong(max)));

    private void SetId(FabricId id)
    {
        if (id.Value == default)
            throw new DomainException(FabricErrors.IdIsRequired());

        Id = id;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(FabricErrors.NameIsRequired());

        Name = Text(name, NameMaxLength)!;
    }

    private void SetPrice(MoneyAmount price)
    {
        if (price.Value < 0 || price.Value > 10_000)
            throw new DomainException(FabricErrors.PriceOutOfRange(0, 10_000));

        Price = price;
    }

    private void SetProviderId(SupplierId providerId)
    {
        if (providerId.Value == default)
            throw new DomainException(FabricErrors.ProviderIdIsRequired());

        ProviderId = providerId;
    }
    #endregion
}
