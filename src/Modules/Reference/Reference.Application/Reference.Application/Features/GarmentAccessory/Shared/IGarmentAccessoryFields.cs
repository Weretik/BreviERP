namespace Reference.Application.Features.GarmentAccessory.Shared;

/// <summary>Editable accessory fields shared by create and update.</summary>
public interface IGarmentAccessoryFields
{
    string Name { get; }
    decimal Price { get; }
    string SupplierName { get; }
    string? Code { get; }
    string? Unit { get; }
    bool IsActive { get; }
    string? Comment { get; }
}
