namespace Reference.Application.Features.GarmentPartOperation.Create.DTOs;

public sealed record CreateGarmentPartOperationCommandRequest(int Id, string GarmentPartName, string Name, decimal Min);
