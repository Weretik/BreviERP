namespace Reference.Api.Contracts.GarmentPartOperations;

/// <summary>An operation card. Fields after <c>Min</c> come from the Calculator sheet «Операції».</summary>
public sealed record CreateGarmentPartOperationRequest(
    int Id,
    string GarmentPartName,
    string Name,
    decimal Min,
    string? Code = null,
    string? Element = null,
    string? Variant = null,
    bool MinutesPerProduct = false,
    bool IsActive = true,
    string? Notes = null);
