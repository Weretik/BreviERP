namespace Reference.Api.Contracts.GarmentParts;

public sealed record CreateGarmentPartRequest(
    int Id,
    string Name,
    int SupplierId = 1,
    string? ContactPerson = null,
    string? PhoneNumber = null);
