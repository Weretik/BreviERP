using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Reference.Domain.GarmentPartOperations.Errors;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;

namespace Reference.Domain.GarmentPartOperations.Entities;

/// <summary>
/// A sewing operation from the Calculator sheet «Операції»: group (<see cref="GarmentPartId"/>), element, variant
/// and the time norm. <see cref="Code"/> (OP-001…) is the stable key for the sheet import. Labour cost is not
/// stored: it is the norm times the minute cost from the sewing pricing settings.
/// </summary>
public class GarmentPartOperation : BaseEntity<GarmentPartOperationId>, IAggregateRoot
{
    public const int CodeMaxLength = 20;
    public const int ElementMaxLength = 150;
    public const int VariantMaxLength = 255;
    public const int NameMaxLength = 255;
    public const int NotesMaxLength = 1000;
    public const decimal MaxMinutes = 10_000m;

    #region Properties
    public string? Code { get; private set; }
    public GarmentPartId GarmentPartId { get; private set; }
    public string? Element { get; private set; }
    public string? Variant { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Min { get; private set; }
    public bool MinutesPerProduct { get; private set; }
    public DateOnly? NormCheckedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }
    #endregion

    #region Constructors
    private GarmentPartOperation() { }

    private GarmentPartOperation(GarmentPartOperationId id, GarmentPartId garmentPartId, string name, decimal min)
    {
        SetId(id);
        SetGarmentPartId(garmentPartId);
        SetName(name);
        SetMin(min);
    }
    #endregion

    #region Factories
    public static GarmentPartOperation Create(
        GarmentPartOperationId id,
        GarmentPartId garmentPartId,
        string name,
        decimal min) => new(id, garmentPartId, name, min);

    public static GarmentPartOperation Create(GarmentPartOperationId id, GarmentPartOperationDetails details)
    {
        var operation = new GarmentPartOperation(id, details.GarmentPartId, details.Name, 0m);
        operation.ApplyDetails(details);
        return operation;
    }

    public void Update(GarmentPartId garmentPartId, string name, decimal min)
    {
        SetGarmentPartId(garmentPartId);
        SetName(name);
        SetMin(min);
    }

    /// <summary>Replaces every card field. «Time per product» operations keep a zero norm.</summary>
    public void ApplyDetails(GarmentPartOperationDetails details)
    {
        Code = NormalizeCode(details.Code);
        SetGarmentPartId(details.GarmentPartId);
        Element = Text(details.Element, ElementMaxLength);
        Variant = Text(details.Variant, VariantMaxLength);
        SetName(details.Name);
        MinutesPerProduct = details.MinutesPerProduct;
        SetMin(details.MinutesPerProduct ? 0m : details.Min);
        NormCheckedAt = details.NormCheckedAt;
        IsActive = details.IsActive;
        Notes = Text(details.Notes, NotesMaxLength);
    }

    public static string? NormalizeCode(string? code) =>
        Text(code, CodeMaxLength)?.ToUpperInvariant();
    #endregion

    #region Setters/Validation
    private static string? Text(string? value, int maxLength) =>
        ReferenceText.Optional(value, maxLength,
            max => new DomainException(GarmentPartOperationErrors.TextTooLong(max)));

    private void SetId(GarmentPartOperationId id)
    {
        if (id.Value == default)
            throw new DomainException(GarmentPartOperationErrors.IdIsRequired());

        Id = id;
    }

    private void SetGarmentPartId(GarmentPartId garmentPartId)
    {
        if (garmentPartId.Value == default)
            throw new DomainException(GarmentPartOperationErrors.GarmentPartIdIsRequired());

        GarmentPartId = garmentPartId;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(GarmentPartOperationErrors.NameIsRequired());

        Name = Text(name, NameMaxLength)!;
    }

    private void SetMin(decimal min)
    {
        if (min < 0)
            throw new DomainException(GarmentPartOperationErrors.MinMustBeNonNegative());
        if (min > MaxMinutes)
            throw new DomainException(GarmentPartOperationErrors.MinOutOfRange(MaxMinutes));

        Min = min;
    }
    #endregion
}
