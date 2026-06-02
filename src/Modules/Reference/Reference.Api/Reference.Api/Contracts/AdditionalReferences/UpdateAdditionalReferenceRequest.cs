namespace Reference.Api.Contracts.AdditionalReferences;

public sealed record UpdateAdditionalReferenceRequest(
    string Name,
    string Key,
    decimal Value,
    string Unit,
    string? Description);
