namespace Reference.Application.Features.GarmentPart.Update.DTOs;

public sealed record UpdateGarmentPartCommandRequest(
    string Name,
    int SupplierId = 1,
    string? ContactPerson = null,
    string? PhoneNumber = null);
