namespace Reference.Application.Features.GarmentPartOperation.Update.DTOs;

public sealed record UpdateGarmentPartOperationCommandRequest(string GarmentPartName, string Name, decimal Min);
