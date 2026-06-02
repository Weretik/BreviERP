namespace Reference.Application.Features.GarmentPartOperation.GetList.DTOs;

public sealed record GarmentPartOperationRowDTO(int Id, string GarmentPartName, string Name, decimal Min);

internal sealed record GarmentPartOperationProjectionDTO(int Id, int GarmentPartId, string Name, decimal Min);
