namespace Reference.Application.Features.GarmentPart.GetList.DTOs;

public sealed record GarmentPartRowDTO(
    int Id,
    string Name,
    string SupplierName,
    string? ContactPerson,
    string? PhoneNumber);
