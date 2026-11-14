using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Results;
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
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }
    #endregion

    #region Constructors
    private GarmentPartOperation() { }
    #endregion

    #region Factories
    public static DomainResult<GarmentPartOperation> Create(
        GarmentPartOperationId id,
        GarmentPartId garmentPartId,
        string name,
        decimal min)
    {
        RequireId(id);
        var check = DomainResult.Combine(CheckPart(garmentPartId), CheckName(name), CheckMin(min));
        if (check.IsFailure) return check.Error!;
        var operation = new GarmentPartOperation { Id = id };
        operation.SetMain(garmentPartId, name, min);
        return operation;
    }

    public static DomainResult<GarmentPartOperation> Create(GarmentPartOperationId id,
        GarmentPartOperationDetails details)
    {
        RequireId(id);
        var check = CheckDetails(details);
        if (check.IsFailure) return check.Error!;
        var operation = new GarmentPartOperation { Id = id };
        operation.SetDetails(details);
        return operation;
    }

    public DomainResult Update(GarmentPartId garmentPartId, string name, decimal min)
    {
        var check = DomainResult.Combine(CheckPart(garmentPartId), CheckName(name), CheckMin(min));
        if (check.IsFailure) return check;
        SetMain(garmentPartId, name, min);
        return DomainResult.Success();
    }

    /// <summary>Replaces every card field. «Time per product» operations keep a zero norm.</summary>
    public DomainResult ApplyDetails(GarmentPartOperationDetails details)
    {
        var check = CheckDetails(details);
        if (check.IsFailure) return check;
        SetDetails(details);
        return DomainResult.Success();
    }

    /// <summary>The upper-case code, or null for none; check it first with <see cref="CheckCode"/>.</summary>
    public static string? NormalizeCode(string? code) => ReferenceText.Optional(code)?.ToUpperInvariant();

    public static DomainResult CheckCode(string? code) => Text(code, CodeMaxLength);
    #endregion

    #region Rules
    public static DomainResult CheckDetails(GarmentPartOperationDetails details) => DomainResult.Combine(
        CheckCode(details.Code),
        CheckPart(details.GarmentPartId),
        Text(details.Element, ElementMaxLength),
        Text(details.Variant, VariantMaxLength),
        CheckName(details.Name),
        CheckMin(details.MinutesPerProduct ? 0m : details.Min),
        Text(details.Notes, NotesMaxLength));

    private static DomainResult CheckPart(GarmentPartId garmentPartId) =>
        garmentPartId.Value == default
            ? GarmentPartOperationErrors.GarmentPartIdIsRequired()
            : DomainResult.Success();

    private static DomainResult CheckName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? GarmentPartOperationErrors.NameIsRequired() : Text(name, NameMaxLength);

    private static DomainResult CheckMin(decimal min) => min switch
    {
        < 0 => GarmentPartOperationErrors.MinMustBeNonNegative(),
        > MaxMinutes => GarmentPartOperationErrors.MinOutOfRange(MaxMinutes),
        _ => DomainResult.Success()
    };

    private static DomainResult Text(string? value, int maxLength) =>
        ReferenceText.Check(value, maxLength, GarmentPartOperationErrors.TextTooLong);

    private static void RequireId(GarmentPartOperationId id)
    {
        if (id.Value == default) throw new ArgumentException("Operation id is required.", nameof(id));
    }

    private void SetMain(GarmentPartId garmentPartId, string name, decimal min)
    {
        GarmentPartId = garmentPartId;
        Name = ReferenceText.Optional(name)!;
        Min = min;
    }

    private void SetDetails(GarmentPartOperationDetails details)
    {
        Code = NormalizeCode(details.Code);
        Element = ReferenceText.Optional(details.Element);
        Variant = ReferenceText.Optional(details.Variant);
        MinutesPerProduct = details.MinutesPerProduct;
        SetMain(details.GarmentPartId, details.Name, details.MinutesPerProduct ? 0m : details.Min);
        IsActive = details.IsActive;
        Notes = ReferenceText.Optional(details.Notes);
    }
    #endregion
}
