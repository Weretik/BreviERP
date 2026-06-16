namespace Reference.Api.Contracts.GarmentParts;

public sealed record UpdateGarmentPartRequest(
    string Name,
    int SupplierId = 1,
    string? ContactPerson = null,
    string? PhoneNumber = null);
