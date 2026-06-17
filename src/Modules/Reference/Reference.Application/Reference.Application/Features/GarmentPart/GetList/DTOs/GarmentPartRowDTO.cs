namespace Reference.Application.Features.GarmentPart.GetList.DTOs;

public sealed record GarmentPartRowDTO(
    int Id,
    string Name,
    int SupplierId,
    string SupplierName,
    string? ContactPerson,
    string? PhoneNumber);
