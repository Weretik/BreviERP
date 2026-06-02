namespace Reference.Api.Contracts.GarmentPartOperations;

public sealed record CreateGarmentPartOperationRequest(int Id, string GarmentPartName, string Name, decimal Min);
