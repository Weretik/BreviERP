namespace Reference.Api.Contracts.AdditionalReferences;

public sealed record CreateAdditionalReferenceRequest(
    int Id,
    string Name,
    string Key,
    decimal Value,
    string Unit,
    string? Description);
