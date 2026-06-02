namespace Reference.Application.Features.AdditionalReference.Create.DTOs;

public sealed record CreateAdditionalReferenceCommandRequest(
    int Id,
    string Name,
    string Key,
    decimal Value,
    string Unit,
    string? Description);
