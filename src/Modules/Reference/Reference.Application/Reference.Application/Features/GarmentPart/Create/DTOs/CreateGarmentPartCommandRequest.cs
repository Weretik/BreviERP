namespace Reference.Application.Features.GarmentPart.Create.DTOs;

public sealed record CreateGarmentPartCommandRequest(
    int Id,
    string Name,
    int SupplierId = 1,
    string? ContactPerson = null,
    string? PhoneNumber = null);
