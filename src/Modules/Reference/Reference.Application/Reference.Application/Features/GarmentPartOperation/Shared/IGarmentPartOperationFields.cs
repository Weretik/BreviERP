namespace Reference.Application.Features.GarmentPartOperation.Shared;

/// <summary>Editable operation fields shared by create and update (Calculator sheet «Операції»).</summary>
public interface IGarmentPartOperationFields
{
    /// <summary>«Група деталей».</summary>
    string GarmentPartName { get; }
    string Name { get; }
    decimal Min { get; }
    string? Code { get; }
    string? Element { get; }
    string? Variant { get; }
    bool MinutesPerProduct { get; }
    bool IsActive { get; }
    string? Notes { get; }
}
